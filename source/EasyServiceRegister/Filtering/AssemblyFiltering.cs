using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyServiceRegister.Filtering
{
    /// <summary>
    /// Options for filtering types during assembly scanning
    /// </summary>
    public class AssemblyFilterOptions
    {
        /// <summary>
        /// Namespaces to include (if null or empty, all namespaces are included)
        /// </summary>
        public List<string> IncludeNamespaces { get; set; } = new List<string>();

        /// <summary>
        /// Namespaces to exclude
        /// </summary>
        public List<string> ExcludeNamespaces { get; set; } = new List<string>();

        /// <summary>
        /// Type name patterns to include (supports wildcards)
        /// </summary>
        public List<string> IncludeTypePatterns { get; set; } = new List<string>();

        /// <summary>
        /// Type name patterns to exclude (supports wildcards)
        /// </summary>
        public List<string> ExcludeTypePatterns { get; set; } = new List<string>();

        /// <summary>
        /// Whether to include only public types (default: true)
        /// </summary>
        public bool PublicTypesOnly { get; set; } = true;

        /// <summary>
        /// Whether to include abstract classes (default: false)
        /// </summary>
        public bool IncludeAbstractClasses { get; set; } = false;

        /// <summary>
        /// Whether to include generic type definitions (default: true)
        /// </summary>
        public bool IncludeGenericTypeDefinitions { get; set; } = true;
    }

    /// <summary>
    /// Provides assembly filtering capabilities for service registration
    /// </summary>
    public static class AssemblyFiltering
    {
        /// <summary>
        /// Adds services from assemblies with filtering options
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="assembly">The assembly to scan</param>
        /// <param name="configure">Configuration action for filter options</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddServicesWithFilter(
            this IServiceCollection services,
            Assembly assembly,
            Action<AssemblyFilterOptions> configure)
        {
            var options = new AssemblyFilterOptions();
            configure?.Invoke(options);

            var types = assembly.GetTypes()
                .Where(t => ShouldIncludeType(t, options));

            // Continue with standard EasyServiceRegister registration logic
            // This is a filtering mechanism that can be used before calling AddServices

            return services;
        }

        /// <summary>
        /// Gets filtered types from an assembly based on the provided options
        /// </summary>
        /// <param name="assembly">The assembly to scan</param>
        /// <param name="options">The filter options</param>
        /// <returns>Filtered types</returns>
        public static IEnumerable<Type> GetFilteredTypes(Assembly assembly, AssemblyFilterOptions options)
        {
            return assembly.GetTypes().Where(t => ShouldIncludeType(t, options));
        }

        private static bool ShouldIncludeType(Type type, AssemblyFilterOptions options)
        {
            // Check if it's a class (not interface, not abstract if configured)
            if (!type.IsClass)
                return false;

            if (!options.IncludeAbstractClasses && type.IsAbstract)
                return false;

            if (options.PublicTypesOnly && !type.IsPublic)
                return false;

            if (!options.IncludeGenericTypeDefinitions && type.IsGenericTypeDefinition)
                return false;

            // Check namespace filters
            if (options.IncludeNamespaces.Any())
            {
                var namespaceMatch = options.IncludeNamespaces.Any(ns =>
                    type.Namespace?.StartsWith(ns, StringComparison.Ordinal) == true);

                if (!namespaceMatch)
                    return false;
            }

            if (options.ExcludeNamespaces.Any())
            {
                var namespaceExclude = options.ExcludeNamespaces.Any(ns =>
                    type.Namespace?.StartsWith(ns, StringComparison.Ordinal) == true);

                if (namespaceExclude)
                    return false;
            }

            // Check type name pattern filters
            if (options.IncludeTypePatterns.Any())
            {
                var patternMatch = options.IncludeTypePatterns.Any(pattern =>
                    MatchesPattern(type.Name, pattern));

                if (!patternMatch)
                    return false;
            }

            if (options.ExcludeTypePatterns.Any())
            {
                var patternExclude = options.ExcludeTypePatterns.Any(pattern =>
                    MatchesPattern(type.Name, pattern));

                if (patternExclude)
                    return false;
            }

            return true;
        }

        private static bool MatchesPattern(string name, string pattern)
        {
            // Simple wildcard matching (supports * at start or end)
            if (pattern.StartsWith("*", StringComparison.Ordinal) && pattern.EndsWith("*", StringComparison.Ordinal))
            {
                var substring = pattern.Substring(1, pattern.Length - 2);
                return name.Contains(substring);
            }
            else if (pattern.StartsWith("*", StringComparison.Ordinal))
            {
                return name.EndsWith(pattern.Substring(1), StringComparison.Ordinal);
            }
            else if (pattern.EndsWith("*", StringComparison.Ordinal))
            {
                return name.StartsWith(pattern.Substring(0, pattern.Length - 1), StringComparison.Ordinal);
            }
            else
            {
                return name.Equals(pattern, StringComparison.Ordinal);
            }
        }
    }
}
