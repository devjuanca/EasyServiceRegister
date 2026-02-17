using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyServiceRegister.Interceptors
{
    /// <summary>
    /// Context provided to registration interceptors
    /// </summary>
    public class RegistrationContext
    {
        /// <summary>
        /// The service type being registered
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// The implementation type being registered
        /// </summary>
        public Type ImplementationType { get; set; }

        /// <summary>
        /// The lifetime of the service
        /// </summary>
        public ServiceLifetime Lifetime { get; set; }

        /// <summary>
        /// The service key (for keyed services)
        /// </summary>
        public object ServiceKey { get; set; }

        /// <summary>
        /// Whether to skip this registration
        /// </summary>
        public bool Skip { get; set; }

        /// <summary>
        /// Custom data that can be attached by interceptors
        /// </summary>
        public object CustomData { get; set; }
    }

    /// <summary>
    /// Interface for registration interceptors
    /// </summary>
    public interface IRegistrationInterceptor
    {
        /// <summary>
        /// Called before a service is registered
        /// </summary>
        /// <param name="context">The registration context</param>
        void BeforeRegistration(RegistrationContext context);

        /// <summary>
        /// Called after a service is registered
        /// </summary>
        /// <param name="context">The registration context</param>
        void AfterRegistration(RegistrationContext context);
    }

    /// <summary>
    /// Base class for registration interceptors
    /// </summary>
    public abstract class RegistrationInterceptorBase : IRegistrationInterceptor
    {
        /// <summary>
        /// Called before a service is registered
        /// </summary>
        /// <param name="context">The registration context</param>
        public virtual void BeforeRegistration(RegistrationContext context)
        {
            // Default implementation does nothing
        }

        /// <summary>
        /// Called after a service is registered
        /// </summary>
        /// <param name="context">The registration context</param>
        public virtual void AfterRegistration(RegistrationContext context)
        {
            // Default implementation does nothing
        }
    }

    /// <summary>
    /// Logging interceptor that logs all service registrations
    /// </summary>
    public class LoggingRegistrationInterceptor : RegistrationInterceptorBase
    {
        private readonly Action<string> _logger;

        /// <summary>
        /// Creates a new logging interceptor
        /// </summary>
        /// <param name="logger">The logging action</param>
        public LoggingRegistrationInterceptor(Action<string> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Logs before registration
        /// </summary>
        public override void BeforeRegistration(RegistrationContext context)
        {
            _logger($"Registering {context.ImplementationType?.Name ?? "Unknown"} as {context.ServiceType?.Name ?? "Unknown"} with {context.Lifetime} lifetime");
        }

        /// <summary>
        /// Logs after registration
        /// </summary>
        public override void AfterRegistration(RegistrationContext context)
        {
            if (context.Skip)
            {
                _logger($"Skipped registration of {context.ImplementationType?.Name ?? "Unknown"}");
            }
            else
            {
                _logger($"Successfully registered {context.ImplementationType?.Name ?? "Unknown"}");
            }
        }
    }

    /// <summary>
    /// Interceptor that validates service registrations
    /// </summary>
    public class ValidationRegistrationInterceptor : RegistrationInterceptorBase
    {
        private readonly Func<RegistrationContext, bool> _validator;

        /// <summary>
        /// Creates a new validation interceptor
        /// </summary>
        /// <param name="validator">The validation function (returns true if valid)</param>
        public ValidationRegistrationInterceptor(Func<RegistrationContext, bool> validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Validates before registration
        /// </summary>
        public override void BeforeRegistration(RegistrationContext context)
        {
            if (!_validator(context))
            {
                context.Skip = true;
            }
        }
    }
}
