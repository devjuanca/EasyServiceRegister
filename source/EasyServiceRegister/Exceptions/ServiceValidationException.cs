using EasyServiceRegister.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyServiceRegister.Exceptions
{
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

}
