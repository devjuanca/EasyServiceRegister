using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace EasyServiceRegister.Diagnostics
{
    /// <summary>
    /// Contains information about a registered service
    /// </summary>
    public class ServiceRegistrationInfo
    {
        /// <summary>
        /// The service type
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// The implementation type
        /// </summary>
        public Type ImplementationType { get; set; }

        /// <summary>
        /// The lifetime of the service
        /// </summary>
        public ServiceLifetime Lifetime { get; set; }

        /// <summary>
        /// The service key (if registered as keyed service)
        /// </summary>
        public object ServiceKey { get; set; }

        /// <summary>
        /// The registration method used (TryAdd or Add)
        /// </summary>
        public string RegistrationMethod { get; set; }

        /// <summary>
        /// The attribute used for registration
        /// </summary>
        public string AttributeUsed { get; set; }

        /// <summary>
        /// The decorators applied to the service
        /// </summary>
        public List<DecoratorInfo> Decorators { get; set; } = new List<DecoratorInfo>();
    }
}