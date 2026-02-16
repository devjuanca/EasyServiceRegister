using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyServiceRegister.Batch
{
    /// <summary>
    /// Provides batch registration capabilities for multiple service types
    /// </summary>
    public static class BatchRegistration
    {
        /// <summary>
        /// Registers multiple implementation types with the same service interface
        /// </summary>
        /// <typeparam name="TService">The service interface type</typeparam>
        /// <param name="services">The service collection</param>
        /// <param name="lifetime">The lifetime for all registrations</param>
        /// <param name="implementationTypes">The implementation types to register</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddMultiple<TService>(
            this IServiceCollection services,
            ServiceLifetime lifetime,
            params Type[] implementationTypes) where TService : class
        {
            foreach (var implementationType in implementationTypes)
            {
                if (!typeof(TService).IsAssignableFrom(implementationType))
                {
                    throw new ArgumentException(
                        $"Type {implementationType.FullName} does not implement {typeof(TService).FullName}",
                        nameof(implementationTypes));
                }

                var descriptor = new ServiceDescriptor(typeof(TService), implementationType, lifetime);
                services.Add(descriptor);
            }

            return services;
        }

        /// <summary>
        /// Registers multiple services with the same lifetime using a fluent API
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="lifetime">The lifetime for all registrations</param>
        /// <returns>A batch registration builder</returns>
        public static BatchRegistrationBuilder WithLifetime(
            this IServiceCollection services,
            ServiceLifetime lifetime)
        {
            return new BatchRegistrationBuilder(services, lifetime);
        }

        /// <summary>
        /// Fluent builder for batch service registration
        /// </summary>
        public class BatchRegistrationBuilder
        {
            private readonly IServiceCollection _services;
            private readonly ServiceLifetime _lifetime;
            private readonly List<(Type ServiceType, Type ImplementationType)> _registrations;

            internal BatchRegistrationBuilder(IServiceCollection services, ServiceLifetime lifetime)
            {
                _services = services;
                _lifetime = lifetime;
                _registrations = new List<(Type, Type)>();
            }

            /// <summary>
            /// Adds a service registration to the batch
            /// </summary>
            /// <typeparam name="TService">The service type</typeparam>
            /// <typeparam name="TImplementation">The implementation type</typeparam>
            /// <returns>The builder for chaining</returns>
            public BatchRegistrationBuilder Add<TService, TImplementation>()
                where TService : class
                where TImplementation : class, TService
            {
                _registrations.Add((typeof(TService), typeof(TImplementation)));
                return this;
            }

            /// <summary>
            /// Adds a service registration to the batch with concrete types
            /// </summary>
            /// <param name="serviceType">The service type</param>
            /// <param name="implementationType">The implementation type</param>
            /// <returns>The builder for chaining</returns>
            public BatchRegistrationBuilder Add(Type serviceType, Type implementationType)
            {
                if (!serviceType.IsAssignableFrom(implementationType))
                {
                    throw new ArgumentException(
                        $"Type {implementationType.FullName} does not implement {serviceType.FullName}");
                }

                _registrations.Add((serviceType, implementationType));
                return this;
            }

            /// <summary>
            /// Completes the batch registration and adds all services to the collection
            /// </summary>
            /// <returns>The service collection</returns>
            public IServiceCollection Build()
            {
                foreach (var (serviceType, implementationType) in _registrations)
                {
                    var descriptor = new ServiceDescriptor(serviceType, implementationType, _lifetime);
                    _services.Add(descriptor);
                }

                return _services;
            }
        }
    }
}
