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
    /// Validates service registrations to detect potential issues
    /// </summary>
    public static class ServiceRegistrationValidator
    {
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

            // Check for duplicate service registrations
#if NET8_0_OR_GREATER
            var serviceGroups = services
                .Where(s => validateFrameworkServices || registeredServiceTypes.Contains(s.ServiceType))
                .GroupBy(s => new { s.ServiceType, IsKeyed = IsKeyedService(s) })
                .Where(g => g.Count() > 1 && !IsEnumerableService(g.Key.ServiceType));

            foreach (var group in serviceGroups)
            {
                string keyInfo = group.Key.IsKeyed ? " (keyed service)" : string.Empty;

                // Duplicate services are just a warning, not a critical error
                issues.Add(new ValidationIssue(
                    $"Duplicate service registration detected for '{group.Key.ServiceType.FullName}'{keyInfo}. Only the last registration will be used at resolution time, which may lead to unexpected behavior. If this is intentional, consider injecting IEnumerable<{group.Key.ServiceType.FullName}> to access all implementations.",
                    ValidationSeverity.Warning,
                    group.Key.ServiceType,
                    group.First().ImplementationType
                ));

            }
#else
        var serviceGroups = services
            .Where(s => validateFrameworkServices || registeredServiceTypes.Contains(s.ServiceType))
            .GroupBy(s => s.ServiceType)
            .Where(g => g.Count() > 1 && !IsEnumerableService(g.Key));

        foreach (var group in serviceGroups)
        {
            // Duplicate services are just a warning, not a critical error
            issues.Add(new ValidationIssue(
                $"Duplicate registration for service {group.Key.FullName}. " +
                $"This may cause unexpected behavior as only the last registration will be used.",
                ValidationSeverity.Warning,
                group.Key,
                group.First().ImplementationType
            ));
        }
#endif

            // Check for services with missing dependencies
            foreach (var descriptor in services.Where(s => registeredServiceTypes.Contains(s.ServiceType)))
            {
                if (descriptor.ImplementationType != null)
                {
                    var missingDependencies = GetMissingDependencies(descriptor.ImplementationType, services);

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

            // Check for lifetime issues (scoped or transient services injected into singletons)
            var singletonServices = services
                .Where(s => s.Lifetime == ServiceLifetime.Singleton)
                .ToList();

            // Get scoped and transient services
            var scopedServices = GetServicesWithLifetime(services, ServiceLifetime.Scoped);

            var transientServices = GetServicesWithLifetime(services, ServiceLifetime.Transient);

            foreach (var singleton in singletonServices)
            {
                if (singleton.ImplementationType != null)
                {
                    // Check for scoped dependencies (this is a serious error)
                    var scopedDependencies = GetLifetimeDependencies(singleton.ImplementationType, services, scopedServices);

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
                    var transientDependencies = GetLifetimeDependencies(singleton.ImplementationType, services, transientServices);

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
                }
            }

            var graph = BuildDependencyGraph(services);

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
            Type implementationType,
            IServiceCollection services,
            List<ServiceInfo> servicesOfLifetime)
        {
            var constructors = implementationType.GetConstructors();

            if (constructors.Length == 0)
            {
                return Enumerable.Empty<ServiceInfo>();
            }

            // Get the constructor with the most parameters (assumed to be the primary constructor)
            var primaryConstructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();

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

        private static bool IsEnumerableService(Type serviceType)
        {
            return serviceType.IsGenericType &&
                   serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        private static IEnumerable<Type> GetMissingDependencies(Type implementationType, IServiceCollection services)
        {
            var constructors = implementationType.GetConstructors();
            if (constructors.Length == 0)
            {
                return Enumerable.Empty<Type>();
            }

            // Get the constructor with the most parameters (assumed to be the primary constructor)
            var primaryConstructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();

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

        private static Dictionary<Type, List<Type>> BuildDependencyGraph(IServiceCollection services)
        {
            var graph = new Dictionary<Type, List<Type>>();

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

                var constructor = implementationType.GetConstructors()
                    .OrderByDescending(c => c.GetParameters().Length)
                    .FirstOrDefault();

                if (constructor == null)
                    continue;

                foreach (var param in constructor.GetParameters())
                {
                    var dependencyType = param.ParameterType;

                    // Ignore framework types or optional types
                    if (IsFrameworkType(dependencyType) || IsOptionalService(dependencyType))
                        continue;

                    dependencies.Add(dependencyType);
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