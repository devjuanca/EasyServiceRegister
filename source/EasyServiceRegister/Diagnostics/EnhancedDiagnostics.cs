using EasyServiceRegister.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyServiceRegister.Diagnostics
{
    /// <summary>
    /// Provides enhanced diagnostic capabilities for service registrations
    /// </summary>
    public static class EnhancedDiagnostics
    {
        /// <summary>
        /// Generates a detailed report of all service registrations
        /// </summary>
        /// <returns>A formatted string containing registration details</returns>
        public static string GenerateRegistrationReport()
        {
            var services = ServicesExtension.GetRegisteredServices().ToList();
            var sb = new StringBuilder();

            sb.AppendLine("=== EasyServiceRegister Diagnostic Report ===");
            sb.AppendLine($"Total Services Registered: {services.Count}");
            sb.AppendLine();

            // Group by lifetime
            var grouped = services.GroupBy(s => s.Lifetime);
            
            foreach (var group in grouped.OrderBy(g => g.Key))
            {
                sb.AppendLine($"--- {group.Key} Services ({group.Count()}) ---");
                
                foreach (var service in group.OrderBy(s => s.ServiceType.Name))
                {
                    sb.AppendLine($"  Service: {service.ServiceType.FullName}");
                    sb.AppendLine($"  Implementation: {service.ImplementationType.FullName}");
                    sb.AppendLine($"  Registration Method: {service.RegistrationMethod}");
                    sb.AppendLine($"  Attribute: {service.AttributeUsed}");
                    
                    if (service.ServiceKey != null)
                    {
                        sb.AppendLine($"  Key: {service.ServiceKey}");
                    }
                    
                    if (service.Decorators.Any())
                    {
                        sb.AppendLine($"  Decorators ({service.Decorators.Count}):");
                        foreach (var decorator in service.Decorators.OrderBy(d => d.Order))
                        {
                            sb.AppendLine($"    - {decorator.DecoratorType.Name} (Order: {decorator.Order})");
                        }
                    }
                    
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets statistics about service registrations
        /// </summary>
        /// <returns>A dictionary containing various statistics</returns>
        public static Dictionary<string, object> GetRegistrationStatistics()
        {
            var services = ServicesExtension.GetRegisteredServices().ToList();
            
            var stats = new Dictionary<string, object>
            {
                ["TotalServices"] = services.Count,
                ["SingletonCount"] = services.Count(s => s.Lifetime == ServiceLifetime.Singleton),
                ["ScopedCount"] = services.Count(s => s.Lifetime == ServiceLifetime.Scoped),
                ["TransientCount"] = services.Count(s => s.Lifetime == ServiceLifetime.Transient),
                ["KeyedServicesCount"] = services.Count(s => s.ServiceKey != null),
                ["DecoratedServicesCount"] = services.Count(s => s.Decorators.Any()),
                ["TotalDecorators"] = services.Sum(s => s.Decorators.Count),
                ["TryAddCount"] = services.Count(s => s.RegistrationMethod == "TryAdd"),
                ["AddCount"] = services.Count(s => s.RegistrationMethod == "Add")
            };

            return stats;
        }

        /// <summary>
        /// Finds services by name pattern
        /// </summary>
        /// <param name="pattern">The pattern to search for (case-insensitive)</param>
        /// <returns>Matching services</returns>
        public static IEnumerable<ServiceRegistrationInfo> FindServicesByName(string pattern)
        {
            var services = ServicesExtension.GetRegisteredServices();
            
            return services.Where(s => 
                s.ServiceType.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
                s.ImplementationType.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all services implementing a specific interface or base class
        /// </summary>
        /// <param name="baseType">The base type to search for</param>
        /// <returns>Services implementing the base type</returns>
        public static IEnumerable<ServiceRegistrationInfo> GetServicesByBaseType(Type baseType)
        {
            var services = ServicesExtension.GetRegisteredServices();
            
            return services.Where(s => 
                baseType.IsAssignableFrom(s.ServiceType) || 
                baseType.IsAssignableFrom(s.ImplementationType));
        }

        /// <summary>
        /// Generates a dependency graph visualization (simple text format)
        /// </summary>
        /// <returns>A text representation of the dependency graph</returns>
        public static string GenerateDependencyGraph()
        {
            var services = ServicesExtension.GetRegisteredServices().ToList();
            var sb = new StringBuilder();

            sb.AppendLine("=== Service Dependency Graph ===");
            sb.AppendLine();

            foreach (var service in services.OrderBy(s => s.ServiceType.Name))
            {
                sb.AppendLine($"{service.ServiceType.Name}");
                sb.AppendLine($"  └─ {service.ImplementationType.Name} [{service.Lifetime}]");
                
                if (service.Decorators.Any())
                {
                    foreach (var decorator in service.Decorators.OrderBy(d => d.Order))
                    {
                        sb.AppendLine($"     └─ Decorator: {decorator.DecoratorType.Name}");
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Exports registration information to CSV format
        /// </summary>
        /// <returns>CSV formatted string</returns>
        public static string ExportToCsv()
        {
            var services = ServicesExtension.GetRegisteredServices().ToList();
            var sb = new StringBuilder();

            sb.AppendLine("ServiceType,ImplementationType,Lifetime,ServiceKey,RegistrationMethod,AttributeUsed,DecoratorCount");

            foreach (var service in services)
            {
                sb.AppendLine($"\"{service.ServiceType.FullName}\",\"{service.ImplementationType.FullName}\",{service.Lifetime},{service.ServiceKey ?? ""},\"{service.RegistrationMethod}\",\"{service.AttributeUsed}\",{service.Decorators.Count}");
            }

            return sb.ToString();
        }
    }
}
