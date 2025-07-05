using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace EasyServiceRegister
{

    /// <summary>
    /// Extension methods for applying the decorator pattern with Microsoft.Extensions.DependencyInjection
    /// </summary>
    internal static class DecoratorExtensions
    {
        /// <summary>
        /// Adds a decorator for a service type.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="serviceType">The service type being decorated.</param>
        /// <param name="decoratorType">The decorator type.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddDecorator(this IServiceCollection services, Type serviceType, Type decoratorType)
        {
            // Find the original descriptor
            var descriptor = services.FirstOrDefault(d => d.ServiceType == serviceType);

            if (descriptor == null)
            {
                throw new InvalidOperationException($"Service of type {serviceType.Name} not registered.");
            }

            // Create a new descriptor with the decorator pattern
            var decoratorDescriptor = new ServiceDescriptor(
                serviceType,
                sp =>
                {
                    // Get the original service implementation
                    object inner;

                    if (descriptor.ImplementationInstance != null)
                    {
                        inner = descriptor.ImplementationInstance;
                    }
                    else if (descriptor.ImplementationFactory != null)
                    {
                        inner = descriptor.ImplementationFactory(sp);
                    }
                    else
                    {
                        inner = ActivatorUtilities.CreateInstance(sp, descriptor.ImplementationType);
                    }

                    // Create the decorator, passing the inner service as a parameter
                    return ActivatorUtilities.CreateInstance(sp, decoratorType, inner);

                }, descriptor.Lifetime);

            // Replace the original registration
            services.Remove(descriptor);

            services.Add(decoratorDescriptor);

            return services;
        }

    }
}