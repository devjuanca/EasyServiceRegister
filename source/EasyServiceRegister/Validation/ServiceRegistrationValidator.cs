using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyServiceRegister.Validation
{

    /// <summary>
    /// Information about a service for lifetime validation
    /// </summary>
    internal class ServiceInfo
    {
        public Type Type { get; set; }
        public bool IsKeyed { get; set; }
    }

    /// <summary>
    /// Exception thrown by EnsureServicesAreValid when validation errors are found.
    /// </summary>
    public class ServiceValidationException : Exception
    {
        /// <summary>
        /// The validation issues that caused the exception.
        /// </summary>
        public IReadOnlyList<ValidationIssue> Issues { get; }

        public ServiceValidationException(IReadOnlyList<ValidationIssue> issues)
            : base(FormatMessage(issues))
        {
            Issues = issues;
        }

        private static string FormatMessage(IReadOnlyList<ValidationIssue> issues)
        {
            var errors = issues.Where(i => i.Severity == ValidationSeverity.Error).ToList();
            var warnings = issues.Where(i => i.Severity == ValidationSeverity.Warning).ToList();

            var lines = new List<string>
            {
                $"Service validation failed with {errors.Count} error(s) and {warnings.Count} warning(s):"
            };

            foreach (var issue in issues)
            {
                lines.Add($"  [{issue.Severity}] {issue.Message}");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }

    /// <summary>
    /// Validates service registrations to detect potential issues
    /// </summary>
    public static class ServiceRegistrationValidator
    {
        /// <summary>
        /// Validates service registrations and throws a <see cref="ServiceValidationException"/>
        /// if any errors are found. Warnings are included in the exception but do not cause a throw on their own.
        /// Call this at startup to fail fast on misconfigured services.
        /// </summary>
        /// <param name="services">The service collection to validate</param>
        /// <param name="validateFrameworkServices">Whether to include framework services in validation</param>
        /// <returns>The IServiceCollection for chaining.</returns>
        /// <exception cref="ServiceValidationException">Thrown when validation errors are detected.</exception>
        public static IServiceCollection EnsureServicesAreValid(
            this IServiceCollection services,
            bool validateFrameworkServices = false)
        {
            var issues = services.ValidateServices(ValidationSeverity.Warning, validateFrameworkServices).ToList();

            var hasErrors = issues.Any(i => i.Severity == ValidationSeverity.Error);

            if (hasErrors)
            {
                throw new ServiceValidationException(issues);
            }

            return services;
        }

        /// <summary>
        /// Validates service registrations and returns any issues found
        /// </summary>
        /// <param name="services">The service collection to validate</param>
        /// <param name="minimumSeverity">The minimum severity level to include in results</param>
        /// <param name="validateFrameworkServices">Whether to include framework services in validation</param>
        /// <returns>A collection of validation issues</returns>
        public static IEnumerable<ValidationIssue> ValidateServices(
            this IServiceCollection services,
            ValidationSeverity minimumSeverity = ValidationSeverity.Warning,
            bool validateFrameworkServices = false)
        {
            var issues = new List<ValidationIssue>();

            // Get all registered services through EasyServiceRegister
            var registeredServices = ServicesExtension.GetRegisteredServices().ToList();

            var registeredServiceTypes = registeredServices.Select(r => r.ServiceType).ToList();

            // Cache constructor info to avoid repeated reflection
            var constructorCache = new Dictionary<Type, ConstructorInfo>();

            // Check for services with missing dependencies
            foreach (var descriptor in services.Where(s => registeredServiceTypes.Contains(s.ServiceType)))
            {
                if (descriptor.ImplementationType != null)
                {
                    var primaryConstructor = GetPrimaryConstructor(descriptor.ImplementationType, constructorCache);
                    if (primaryConstructor == null)
                        continue;

                    var missingDependencies = GetMissingDependencies(primaryConstructor, services);

                    foreach (var dependency in missingDependencies)
                    {
                        // Missing dependencies are errors as they'll cause runtime exceptions
                        issues.Add(new ValidationIssue(
                            $"Service {descriptor.ImplementationType.FullName} depends on {dependency.FullName} which is not registered in the container.",
                            ValidationSeverity.Error,
                            descriptor.ServiceType,
                            descriptor.ImplementationType
                        ));
                    }
                }
            }

            // Check for disposable transient services (potential memory leak)
            foreach (var descriptor in services.Where(s => s.Lifetime == ServiceLifetime.Transient))
            {
                var implType = descriptor.ImplementationType;
                if (implType != null && typeof(IDisposable).IsAssignableFrom(implType))
                {
                    // Only warn for services registered through EasyServiceRegister
                    if (registeredServiceTypes.Contains(descriptor.ServiceType))
                    {
                        issues.Add(new ValidationIssue(
                            $"Transient service {implType.FullName} implements IDisposable. Transient disposable services are not tracked by the DI container and will not be disposed, which may cause memory leaks. Consider changing the lifetime to Scoped or Singleton, or managing disposal manually.",
                            ValidationSeverity.Warning,
                            descriptor.ServiceType,
                            implType
                        ));
                    }
                }
            }

            // Check for lifetime issues (scoped or transient services injected into singletons)
            var singletonServices = services
                .Where(s => s.Lifetime == ServiceLifetime.Singleton)
                .ToList();

            // Build lookup for captive dependency chain detection
            var lifetimeLookup = new Dictionary<Type, ServiceLifetime>();
            foreach (var desc in services)
            {
                if (desc.ImplementationType != null)
                {
                    // Last wins
                    lifetimeLookup[desc.ServiceType] = desc.Lifetime;
                }
            }

            // Get scoped and transient services
            var scopedServices = GetServicesWithLifetime(services, ServiceLifetime.Scoped);

            var transientServices = GetServicesWithLifetime(services, ServiceLifetime.Transient);

            foreach (var singleton in singletonServices)
            {
                if (singleton.ImplementationType != null)
                {
                    var primaryConstructor = GetPrimaryConstructor(singleton.ImplementationType, constructorCache);
                    if (primaryConstructor == null)
                        continue;

                    // Check for scoped dependencies (this is a serious error)
                    var scopedDependencies = GetLifetimeDependencies(primaryConstructor, services, scopedServices);

                    foreach (var scopedDependency in scopedDependencies)
                    {
                        string keyInfo = string.Empty;
#if NET8_0_OR_GREATER
                        if (scopedDependency.IsKeyed)
                        {
                            keyInfo = " (keyed service)";
                        }
#endif

                        issues.Add(new ValidationIssue(
                            $"Singleton service {singleton.ImplementationType.FullName} depends on scoped service {scopedDependency.Type.FullName}{keyInfo}, which will fail at runtime because the DI container does not allow scoped services to be resolved from the root scope.",
                            ValidationSeverity.Error,
                            singleton.ServiceType,
                            singleton.ImplementationType
                        ));
                    }

                    // Check for transient dependencies (this is a warning)
                    var transientDependencies = GetLifetimeDependencies(primaryConstructor, services, transientServices);

                    foreach (var transientDependency in transientDependencies)
                    {
                        string keyInfo = string.Empty;
#if NET8_0_OR_GREATER
                        if (transientDependency.IsKeyed)
                        {
                            keyInfo = " (keyed service)";
                        }
#endif
                        issues.Add(new ValidationIssue(
                            $"Singleton service {singleton.ImplementationType.FullName} depends on transient service {transientDependency.Type.FullName}{keyInfo}. This is allowed, but the transient will be instantiated only once when the singleton is created and then reused, which may not match the intended transient lifetime semantics.",

                            ValidationSeverity.Warning,
                            singleton.ServiceType,
                            singleton.ImplementationType
                        ));
                    }

                    // Check for captive dependencies through intermediate singletons
                    // A(singleton) -> B(singleton) -> C(scoped) is a captive dependency chain
                    DetectCaptiveDependencyChain(singleton.ImplementationType, services, constructorCache, lifetimeLookup, issues, new HashSet<Type>());
                }
            }

            var graph = BuildDependencyGraph(services, constructorCache);

            var cycles = DetectCycles(graph);

            foreach (var cycle in cycles)
            {
                var message = "Dependency cycle detected: " + string.Join(" -> ", cycle.Select(t => services.FirstOrDefault(s => s.ServiceType == t)?.ImplementationType != null &&
                         services.FirstOrDefault(s => s.ServiceType == t)?.ImplementationType != t
                   ? $"{t.Name} ({services.FirstOrDefault(s => s.ServiceType == t)?.ImplementationType.Name})"
                   : t.Name));

                issues.Add(new ValidationIssue(
                    message,
                    ValidationSeverity.Error,
                    cycle.First(),
                    null
                ));
            }


            return issues.Where(i => i.Severity >= minimumSeverity);
        }

        private static ConstructorInfo GetPrimaryConstructor(Type implementationType, Dictionary<Type, ConstructorInfo> cache)
        {
            if (cache.TryGetValue(implementationType, out var cached))
                return cached;

            var constructors = implementationType.GetConstructors();
            var primary = constructors.Length == 0
                ? null
                : constructors.OrderByDescending(c => c.GetParameters().Length).First();

            cache[implementationType] = primary;
            return primary;
        }

        private static List<ServiceInfo> GetServicesWithLifetime(IServiceCollection services, ServiceLifetime lifetime)
        {
            var result = new List<ServiceInfo>();

            foreach (var service in services.Where(s => s.Lifetime == lifetime))
            {
                var serviceInfo = new ServiceInfo
                {
                    Type = service.ServiceType,
                    IsKeyed = false
                };

#if NET8_0_OR_GREATER
                serviceInfo.IsKeyed = IsKeyedService(service);
#endif

                result.Add(serviceInfo);
            }

            return result;
        }

#if NET8_0_OR_GREATER
        private static bool IsKeyedService(ServiceDescriptor descriptor)
        {
            return descriptor.IsKeyedService;
        }
#endif

        private static IEnumerable<ServiceInfo> GetLifetimeDependencies(
            ConstructorInfo primaryConstructor,
            IServiceCollection services,
            List<ServiceInfo> servicesOfLifetime)
        {
            var dependencies = new List<ServiceInfo>();

            foreach (var parameter in primaryConstructor.GetParameters())
            {
                var parameterType = parameter.ParameterType;

                // Check for regular service dependencies
#if NET8_0_OR_GREATER
                var matchingServices = servicesOfLifetime
                    .Where(s => s.Type == parameterType && !s.IsKeyed)
                    .ToList();
#else
            var matchingServices = servicesOfLifetime
                .Where(s => s.Type == parameterType)
                .ToList();
#endif

                dependencies.AddRange(matchingServices);

                // Check for keyed service dependencies
#if NET8_0_OR_GREATER
                // Look for [FromKeyedServices] attribute
                var hasKeyedServiceAttr = parameter.GetCustomAttributes()
                    .Any(a => a.GetType().Name == "FromKeyedServicesAttribute");

                if (hasKeyedServiceAttr)
                {
                    // If we found a keyed service attribute, check for any keyed service of this type
                    var matchingKeyedServices = servicesOfLifetime
                        .Where(s => s.Type == parameterType && s.IsKeyed)
                        .ToList();

                    dependencies.AddRange(matchingKeyedServices);
                }
#endif
            }

            return dependencies;
        }

        private static IEnumerable<Type> GetMissingDependencies(ConstructorInfo primaryConstructor, IServiceCollection services)
        {
            var missingDependencies = new List<Type>();

            foreach (var parameter in primaryConstructor.GetParameters())
            {
                var parameterType = parameter.ParameterType;

                // Check if there's a regular service registration
                bool hasService = services.Any(s => s.ServiceType == parameterType);

                // Check if there's a keyed service registration
#if NET8_0_OR_GREATER
                var hasKeyedServiceAttr = parameter.GetCustomAttributes()
                    .Any(a => a.GetType().Name == "FromKeyedServicesAttribute");

                if (hasKeyedServiceAttr)
                {
                    // Just check if any keyed service exists for this type
                    hasService = hasService || services.Any(s =>
                        s.ServiceType == parameterType && IsKeyedService(s));
                }
#endif

                if (!hasService &&
                    !IsFrameworkType(parameterType) &&
                    !IsOptionalService(parameterType))
                {
                    missingDependencies.Add(parameterType);
                }
            }

            return missingDependencies;
        }

        private static bool IsFrameworkType(Type type)
        {
            // Skip validation for framework types or primitive types
            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(DateTime) ||
                   type.Namespace?.StartsWith("System") == true ||
                   type.Namespace?.StartsWith("Microsoft") == true;
        }

        private static bool IsOptionalService(Type type)
        {
            // Check if type is optional (e.g., wrapped in Nullable<T> or Optional<T>)
            return type.IsGenericType &&
                  (type.GetGenericTypeDefinition() == typeof(Nullable<>) ||
                   type.Name.Contains("Optional"));
        }

        private static void DetectCaptiveDependencyChain(
            Type rootSingletonImpl,
            IServiceCollection services,
            Dictionary<Type, ConstructorInfo> constructorCache,
            Dictionary<Type, ServiceLifetime> lifetimeLookup,
            List<ValidationIssue> issues,
            HashSet<Type> visited)
        {
            if (!visited.Add(rootSingletonImpl))
                return;

            var constructor = GetPrimaryConstructor(rootSingletonImpl, constructorCache);
            if (constructor == null)
                return;

            foreach (var param in constructor.GetParameters())
            {
                var depType = param.ParameterType;

                if (IsFrameworkType(depType) || IsOptionalService(depType))
                    continue;

                // Find the descriptor for this dependency
                var depDescriptor = services.LastOrDefault(s => s.ServiceType == depType);
                if (depDescriptor?.ImplementationType == null)
                    continue;

                // Only follow singleton intermediaries
                if (depDescriptor.Lifetime != ServiceLifetime.Singleton)
                    continue;

                // Check this singleton's own dependencies for captive scoped services
                var innerConstructor = GetPrimaryConstructor(depDescriptor.ImplementationType, constructorCache);
                if (innerConstructor == null)
                    continue;

                foreach (var innerParam in innerConstructor.GetParameters())
                {
                    var innerDepType = innerParam.ParameterType;
                    if (IsFrameworkType(innerDepType) || IsOptionalService(innerDepType))
                        continue;

                    if (lifetimeLookup.TryGetValue(innerDepType, out var innerLifetime) && innerLifetime == ServiceLifetime.Scoped)
                    {
                        issues.Add(new ValidationIssue(
                            $"Captive dependency detected: singleton {rootSingletonImpl.FullName} -> singleton {depDescriptor.ImplementationType.FullName} -> scoped {innerDepType.FullName}. The scoped service will be captured by the singleton chain and will not be disposed per scope.",
                            ValidationSeverity.Error,
                            depDescriptor.ServiceType,
                            depDescriptor.ImplementationType
                        ));
                    }
                }

                // Recurse deeper through singleton intermediaries
                DetectCaptiveDependencyChain(depDescriptor.ImplementationType, services, constructorCache, lifetimeLookup, issues, visited);
            }
        }

        private static Dictionary<Type, List<Type>> BuildDependencyGraph(IServiceCollection services, Dictionary<Type, ConstructorInfo> constructorCache)
        {
            var graph = new Dictionary<Type, List<Type>>();

            // Build a lookup from service type (interface) to implementation type
            // so we can resolve interface dependencies to their implementations
            var serviceToImpl = new Dictionary<Type, Type>();
            foreach (var descriptor in services)
            {
                if (descriptor.ImplementationType != null)
                {
                    // Last registration wins (same as DI container behavior)
                    serviceToImpl[descriptor.ServiceType] = descriptor.ImplementationType;
                }
            }

            foreach (var descriptor in services)
            {
                if (descriptor.ImplementationType == null)
                    continue;

                var implementationType = descriptor.ImplementationType;

                if (!graph.TryGetValue(implementationType, out var dependencies))
                {
                    dependencies = new List<Type>();

                    graph[implementationType] = dependencies;
                }

                var constructor = GetPrimaryConstructor(implementationType, constructorCache);

                if (constructor == null)
                    continue;

                foreach (var param in constructor.GetParameters())
                {
                    var dependencyType = param.ParameterType;

                    // Ignore framework types or optional types
                    if (IsFrameworkType(dependencyType) || IsOptionalService(dependencyType))
                        continue;

                    // Resolve service type to implementation type for cycle detection
                    if (serviceToImpl.TryGetValue(dependencyType, out var resolvedImpl))
                    {
                        dependencies.Add(resolvedImpl);
                    }
                    else
                    {
                        dependencies.Add(dependencyType);
                    }
                }
            }

            return graph;
        }

        private static List<List<Type>> DetectCycles(Dictionary<Type, List<Type>> graph)
        {
            var visited = new HashSet<Type>();

            var stack = new HashSet<Type>();

            var path = new Stack<Type>();

            var cycles = new List<List<Type>>();

            void Visit(Type node)
            {
                if (stack.Contains(node))
                {
                    var cycle = path.Reverse().SkipWhile(n => n != node).ToList();

                    cycle.Add(node);

                    cycles.Add(cycle);

                    return;
                }

                if (visited.Contains(node))
                {
                    return;
                }

                visited.Add(node);

                stack.Add(node);

                path.Push(node);

                if (graph.TryGetValue(node, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        Visit(neighbor);
                    }
                }

                path.Pop();

                stack.Remove(node);
            }

            foreach (var node in graph.Keys)
            {
                Visit(node);
            }

            return cycles;
        }

    }
}
