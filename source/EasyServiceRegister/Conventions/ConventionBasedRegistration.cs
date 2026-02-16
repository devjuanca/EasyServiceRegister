using Microsoft.Extensions.DependencyInjection;
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
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddServicesByConvention(
            this IServiceCollection services,
            Assembly assembly,
            ServiceLifetime lifetime = ServiceLifetime.Scoped,
            string interfaceNamePattern = "I{0}")
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic);

            foreach (var implementationType in types)
            {
                // Look for interface with conventional name
                var expectedInterfaceName = string.Format(interfaceNamePattern, implementationType.Name);
                var serviceInterface = implementationType.GetInterfaces()
                    .FirstOrDefault(i => i.Name == expectedInterfaceName);

                if (serviceInterface != null)
                {
                    var descriptor = new ServiceDescriptor(serviceInterface, implementationType, lifetime);
                    services.Add(descriptor);
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
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddServicesBySuffix(
            this IServiceCollection services,
            Assembly assembly,
            string suffix,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic && t.Name.EndsWith(suffix, StringComparison.Ordinal));

            foreach (var implementationType in types)
            {
                // Register with the first interface, or self-register if no interfaces
                var serviceInterface = implementationType.GetInterfaces().FirstOrDefault();
                var serviceType = serviceInterface ?? implementationType;

                var descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime);
                services.Add(descriptor);
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
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddServicesByNamespace(
            this IServiceCollection services,
            Assembly assembly,
            string @namespace,
            ServiceLifetime lifetime = ServiceLifetime.Scoped,
            bool includeSubNamespaces = true)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic)
                .Where(t => includeSubNamespaces
                    ? t.Namespace?.StartsWith(@namespace, StringComparison.Ordinal) == true
                    : t.Namespace == @namespace);

            foreach (var implementationType in types)
            {
                var serviceInterface = implementationType.GetInterfaces().FirstOrDefault();
                var serviceType = serviceInterface ?? implementationType;

                var descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime);
                services.Add(descriptor);
            }

            return services;
        }
    }
}
