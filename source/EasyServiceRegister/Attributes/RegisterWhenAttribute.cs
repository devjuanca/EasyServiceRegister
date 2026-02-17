using System;

namespace EasyServiceRegister.Attributes
{
    /// <summary>
    /// Marks a class to be registered only when a specific condition is met
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterWhenAttribute : Attribute
    {
        /// <summary>
        /// The environment name(s) in which this service should be registered (e.g., "Development", "Production")
        /// </summary>
        internal string[] Environments { get; }

        /// <summary>
        /// Whether to register this service only in the specified environments (true) or exclude from them (false)
        /// </summary>
        internal bool IncludeEnvironments { get; }

        /// <summary>
        /// Marks a service to be registered conditionally based on environment
        /// </summary>
        /// <param name="environments">Environment names where this service should be registered</param>
        /// <param name="includeEnvironments">True to include only in these environments, false to exclude from them</param>
        public RegisterWhenAttribute(string[] environments, bool includeEnvironments = true)
        {
            Environments = environments ?? throw new ArgumentNullException(nameof(environments));
            IncludeEnvironments = includeEnvironments;
        }

        /// <summary>
        /// Marks a service to be registered conditionally based on a single environment
        /// </summary>
        /// <param name="environment">Environment name where this service should be registered</param>
        /// <param name="includeEnvironment">True to include only in this environment, false to exclude from it</param>
        public RegisterWhenAttribute(string environment, bool includeEnvironment = true)
        {
            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentException("Environment cannot be null or empty", nameof(environment));

            Environments = new[] { environment };
            IncludeEnvironments = includeEnvironment;
        }
    }
}
