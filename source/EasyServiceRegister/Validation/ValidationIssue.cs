using System;

namespace EasyServiceRegister.Validation
{
    /// <summary>
    /// Represents the severity of a validation issue
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>
        /// Potential issue that might cause problems in some scenarios
        /// </summary>
        Warning,

        /// <summary>
        /// Serious issue that will likely cause runtime errors
        /// </summary>
        Error
    }

    /// <summary>
    /// Represents a service registration validation issue
    /// </summary>
    public class ValidationIssue
    {
        /// <summary>
        /// The validation message describing the issue
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The severity of the issue
        /// </summary>
        public ValidationSeverity Severity { get; set; }

        /// <summary>
        /// The service type with the issue
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// The implementation type with the issue
        /// </summary>
        public Type ImplementationType { get; set; }

        /// <summary>
        /// Creates a new validation issue
        /// </summary>
        public ValidationIssue(string message, ValidationSeverity severity, Type serviceType = null, Type implementationType = null)
        {
            Message = message;
            Severity = severity;
            ServiceType = serviceType;
            ImplementationType = implementationType;
        }

        public override string ToString()
        {
            return $"[{Severity}] {Message}";
        }
    }
}