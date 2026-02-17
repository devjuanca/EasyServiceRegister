using EasyServiceRegister.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace EasyServiceRegister.Conventions
{
    /// <summary>
    /// Provides convention-based service registration capabilities
    /// </summary>
    public static class ConventionBasedRegistration
    {
        /// <summary>
        /// Registers services based on naming conventions (e.g., IServiceName -> ServiceName)
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="assembly">Assembly to scan for services</param>
        /// <param name="lifetime">The lifetime to register services with</param>
        /// <param name="interfaceNamePattern">Pattern for interface names (default: "I{0}")</param>
        /// <param name="skipTypesWithAttributes">If true, skips types that have EasyServiceRegister registration attributes (default: true)</param>
        /// <param name="useTryAdd">If true, uses TryAdd instead of Add to avoid duplicate registrations (default: true)</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddServicesByConvention(
            this IServiceCollection services,
            Assembly assembly,
            ServiceLifetime lifetime = ServiceLifetime.Scoped,
            string interfaceNamePattern = "I{0}",
            bool skipTypesWithAttributes = true,
            bool useTryAdd = true)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic);

            foreach (var implementationType in types)
            {
                // Skip types with registration attributes if requested
                if (skipTypesWithAttributes && HasRegistrationAttribute(implementationType))
                {
                    continue;
                }

                // Look for interface with conventional name
                var expectedInterfaceName = string.Format(interfaceNamePattern, implementationType.Name);
                var serviceInterface = implementationType.GetInterfaces()
                    .FirstOrDefault(i => i.Name == expectedInterfaceName);

                if (serviceInterface != null)
                {
                    var descriptor = new ServiceDescriptor(serviceInterface, implementationType, lifetime);
                    if (useTryAdd)
                    {
                        services.TryAdd(descriptor);
                    }
                    else
                    {
                        services.Add(descriptor);
                    }
                }
            }

            return services;
        }

        /// <summary>
        /// Registers services based on a suffix pattern (e.g., classes ending with "Service" or "Repository")
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="assembly">Assembly to scan for services</param>
        /// <param name="suffix">The suffix to look for (e.g., "Service", "Repository")</param>
        /// <param name="lifetime">The lifetime to register services with</param>
        /// <param name="skipTypesWithAttributes">If true, skips types that have EasyServiceRegister registration attributes (default: true)</param>
        /// <param name="useTryAdd">If true, uses TryAdd instead of Add to avoid duplicate registrations (default: true)</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddServicesBySuffix(
            this IServiceCollection services,
            Assembly assembly,
            string suffix,
            ServiceLifetime lifetime = ServiceLifetime.Scoped,
            bool skipTypesWithAttributes = true,
            bool useTryAdd = true)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic && t.Name.EndsWith(suffix, StringComparison.Ordinal));

            foreach (var implementationType in types)
            {
                // Skip types with registration attributes if requested
                if (skipTypesWithAttributes && HasRegistrationAttribute(implementationType))
                {
                    continue;
                }

                // Register with the first interface, or self-register if no interfaces
                var serviceInterface = implementationType.GetInterfaces().FirstOrDefault();
                var serviceType = serviceInterface ?? implementationType;

                var descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime);
                if (useTryAdd)
                {
                    services.TryAdd(descriptor);
                }
                else
                {
                    services.Add(descriptor);
                }
            }

            return services;
        }

        /// <summary>
        /// Registers services in a specific namespace with a given lifetime
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="assembly">Assembly to scan for services</param>
        /// <param name="namespace">The namespace to scan</param>
        /// <param name="lifetime">The lifetime to register services with</param>
        /// <param name="includeSubNamespaces">Whether to include sub-namespaces</param>
        /// <param name="skipTypesWithAttributes">If true, skips types that have EasyServiceRegister registration attributes (default: true)</param>
        /// <param name="useTryAdd">If true, uses TryAdd instead of Add to avoid duplicate registrations (default: true)</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddServicesByNamespace(
            this IServiceCollection services,
            Assembly assembly,
            string @namespace,
            ServiceLifetime lifetime = ServiceLifetime.Scoped,
            bool includeSubNamespaces = true,
            bool skipTypesWithAttributes = true,
            bool useTryAdd = true)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic)
                .Where(t => includeSubNamespaces
                    ? t.Namespace?.StartsWith(@namespace, StringComparison.Ordinal) == true
                    : t.Namespace == @namespace);

            foreach (var implementationType in types)
            {
                // Skip types with registration attributes if requested
                if (skipTypesWithAttributes && HasRegistrationAttribute(implementationType))
                {
                    continue;
                }

                var serviceInterface = implementationType.GetInterfaces().FirstOrDefault();
                var serviceType = serviceInterface ?? implementationType;

                var descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime);
                if (useTryAdd)
                {
                    services.TryAdd(descriptor);
                }
                else
                {
                    services.Add(descriptor);
                }
            }

            return services;
        }

        /// <summary>
        /// Checks if a type has any EasyServiceRegister registration attributes
        /// </summary>
        private static bool HasRegistrationAttribute(Type type)
        {
            return type.GetCustomAttribute<RegisterAsSingletonAttribute>() != null
                || type.GetCustomAttribute<RegisterAsScopedAttribute>() != null
                || type.GetCustomAttribute<RegisterAsTransientAttribute>() != null
#if NET8_0_OR_GREATER
                || type.GetCustomAttribute<RegisterAsSingletonKeyedAttribute>() != null
                || type.GetCustomAttribute<RegisterAsScopedKeyedAttribute>() != null
                || type.GetCustomAttribute<RegisterAsTransientKeyedAttribute>() != null
#endif
                ;
        }
    }
}
