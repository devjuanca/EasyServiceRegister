using EasyServiceRegister.Attributes;
using EasyServiceRegister.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyServiceRegister
{
    public static class ServicesExtension
    {
        private static readonly List<ServiceRegistrationInfo> _registrationLog = new List<ServiceRegistrationInfo>();

        /// <summary>
        /// Gets information about all services registered through EasyServiceRegister
        /// </summary>
        /// <returns>A collection of service registration information</returns>
        public static IEnumerable<ServiceRegistrationInfo> GetRegisteredServices()
        {
            return _registrationLog.AsReadOnly();
        }

        /// <summary>
        /// Gets information about registered services filtered by type or lifetime
        /// </summary>
        /// <param name="serviceType">Optional service type to filter by</param>
        /// <param name="lifetime">Optional lifetime to filter by</param>
        /// <returns>Filtered collection of service registration information</returns>
        public static IEnumerable<ServiceRegistrationInfo> GetRegisteredServices(Type serviceType = null, ServiceLifetime? lifetime = null)
        {
            var result = _registrationLog.AsEnumerable();

            if (serviceType != null)
                result = result.Where(r => r.ServiceType == serviceType);

            if (lifetime.HasValue)
                result = result.Where(r => r.Lifetime == lifetime.Value);

            return result;
        }

        /// <summary>
        /// Scans the assemblies containing the specified marker types and registers services
        /// based on custom attributes such as RegisterAsSingleton, RegisterAsScoped, RegisterAsTransient,
        /// and their keyed variants.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="handlerAssemblyMarkerTypes">Types used to locate assemblies to scan for service registrations.</param>
        /// <returns>The IServiceCollection for chaining.</returns>
        public static IServiceCollection AddServices(this IServiceCollection services, params Type[] handlerAssemblyMarkerTypes)
        {
            try
            {
                foreach (var markerType in handlerAssemblyMarkerTypes)
                {
                    Assembly assembly = Assembly.GetAssembly(markerType) ?? throw new Exception("Assembly for this type does not exist");

                    RegisterSingletonServices(services, assembly);
                    RegisterScopedServices(services, assembly);
                    RegisterTransientServices(services, assembly);

#if NET8_0_OR_GREATER
                    RegisterKeyedScopedService(services, assembly);
                    RegisterKeyedSingletonService(services, assembly);
                    RegisterKeyedTransientService(services, assembly);
#endif
                    ApplyDecorators(services, assembly);
                }

                return services;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Scans the provided assemblies and registers services
        /// based on custom attributes such as RegisterAsSingleton, RegisterAsScoped, RegisterAsTransient,
        /// and their keyed variants.
        /// Scans the provided assemblies and registers services
        /// based on custom attributes such as RegisterAsSingleton, RegisterAsScoped, RegisterAsTransient,
        /// and their keyed variants.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddServices(this IServiceCollection services, params Assembly[] assemblies)
        {
            try
            {
                foreach (var assembly in assemblies)
                {
                    RegisterSingletonServices(services, assembly);
                    RegisterScopedServices(services, assembly);
                    RegisterTransientServices(services, assembly);
#if NET8_0_OR_GREATER
                    RegisterKeyedScopedService(services, assembly);
                    RegisterKeyedSingletonService(services, assembly);
                    RegisterKeyedTransientService(services, assembly);
#endif
                    ApplyDecorators(services, assembly);
                }
                return services;
            }
            catch
            {
                throw;
            }
        }

        private static void RegisterSingletonServices(IServiceCollection services, Assembly assembly)
        {
            var singletonServicesToRegister = assembly.DefinedTypes.Where(a => a.GetCustomAttribute<RegisterAsSingletonAttribute>() != null);

            foreach (var implementationType in singletonServicesToRegister)
            {
                var typeInfo = implementationType.GetTypeInfo();
                var registerBehavior = typeInfo.GetCustomAttribute<RegisterAsSingletonAttribute>()?.UseTryAddSingleton ?? true;
                var serviceInterface = typeInfo.GetCustomAttribute<RegisterAsSingletonAttribute>()?.ServiceInterface;
                RegisterService(services, serviceInterface, implementationType, registerBehavior, ServiceLifetime.Singleton);
            }
        }

        private static void RegisterScopedServices(IServiceCollection services, Assembly assembly)
        {
            var scopedServicesToRegister = assembly.DefinedTypes.Where(a => a.GetCustomAttribute<RegisterAsScopedAttribute>() != null);

            foreach (var implementationType in scopedServicesToRegister)
            {
                var typeInfo = implementationType.GetTypeInfo();
                var registerBehavior = typeInfo.GetCustomAttribute<RegisterAsScopedAttribute>()?.UseTryAddScoped ?? true;
                var serviceInterface = typeInfo.GetCustomAttribute<RegisterAsScopedAttribute>()?.ServiceInterface;
                RegisterService(services, serviceInterface, implementationType, registerBehavior, ServiceLifetime.Scoped);
            }
        }

        private static void RegisterTransientServices(IServiceCollection services, Assembly assembly)
        {
            var transientServicesToRegister = assembly.DefinedTypes.Where(a => a.GetCustomAttribute<RegisterAsTransientAttribute>() != null);

            foreach (var implementationType in transientServicesToRegister)
            {
                var typeInfo = implementationType.GetTypeInfo();
                var registerBehavior = typeInfo.GetCustomAttribute<RegisterAsTransientAttribute>()?.UseTryAddTransient ?? false;
                var serviceInterface = typeInfo.GetCustomAttribute<RegisterAsTransientAttribute>()?.ServiceInterface;
                RegisterService(services, serviceInterface, implementationType, registerBehavior, ServiceLifetime.Transient);
            }
        }

        private static void RegisterKeyedScopedService(IServiceCollection services, Assembly assembly)
        {
            var scopedServicesToRegister = assembly.DefinedTypes.Where(a => a.GetCustomAttribute<RegisterAsScopedKeyedAttribute>() != null);

            foreach (var implementationType in scopedServicesToRegister)
            {
                var typeInfo = implementationType.GetTypeInfo();
                var registerBehavior = typeInfo.GetCustomAttribute<RegisterAsScopedKeyedAttribute>()?.UseTryAddScoped ?? false;
                var key = typeInfo.GetCustomAttribute<RegisterAsScopedKeyedAttribute>().Key;
                var serviceInterface = typeInfo.GetCustomAttribute<RegisterAsScopedKeyedAttribute>()?.ServiceInterface;
                RegisterKeyedService(services, serviceInterface, implementationType, registerBehavior, key, ServiceLifetime.Scoped);
            }
        }

        private static void RegisterKeyedSingletonService(IServiceCollection services, Assembly assembly)
        {
            var singletonServicesToRegister = assembly.DefinedTypes.Where(a => a.GetCustomAttribute<RegisterAsSingletonKeyedAttribute>() != null);

            foreach (var implementationType in singletonServicesToRegister)
            {
                var typeInfo = implementationType.GetTypeInfo();
                var registerBehavior = typeInfo.GetCustomAttribute<RegisterAsSingletonKeyedAttribute>()?.UseTryAddSingleton ?? true;
                var key = typeInfo.GetCustomAttribute<RegisterAsSingletonKeyedAttribute>().Key;
                var serviceInterface = typeInfo.GetCustomAttribute<RegisterAsSingletonKeyedAttribute>()?.ServiceInterface;
                RegisterKeyedService(services, serviceInterface, implementationType, registerBehavior, key, ServiceLifetime.Singleton);
            }
        }

        private static void RegisterKeyedTransientService(IServiceCollection services, Assembly assembly)
        {
            var transientServicesToRegister = assembly.DefinedTypes.Where(a => a.GetCustomAttribute<RegisterAsTransientKeyedAttribute>() != null);

            foreach (var implementationType in transientServicesToRegister)
            {
                var typeInfo = implementationType.GetTypeInfo();
                var registerBehavior = typeInfo.GetCustomAttribute<RegisterAsTransientKeyedAttribute>()?.UseTryAddTransient ?? false;
                var key = typeInfo.GetCustomAttribute<RegisterAsTransientKeyedAttribute>().Key;
                var serviceInterface = typeInfo.GetCustomAttribute<RegisterAsTransientKeyedAttribute>()?.ServiceInterface;
                RegisterKeyedService(services, serviceInterface, implementationType, registerBehavior, key, ServiceLifetime.Transient);
            }
        }

        private static void RegisterService(IServiceCollection services, Type abstractionType, Type implementationType, bool useTryAdd, ServiceLifetime lifetime)
        {
            var typeInfo = implementationType.GetTypeInfo();

            if (!typeInfo.ImplementedInterfaces.Any())
            {
                if (useTryAdd)
                {
                    services.TryAdd(new ServiceDescriptor(implementationType, implementationType, lifetime: lifetime));
                }
                else
                {
                    services.Add(new ServiceDescriptor(implementationType, implementationType, lifetime: lifetime));
                }

                _registrationLog.Add(new ServiceRegistrationInfo
                {
                    ServiceType = implementationType,
                    ImplementationType = implementationType,
                    Lifetime = lifetime,
                    RegistrationMethod = useTryAdd ? "TryAdd" : "Add",
                    AttributeUsed = GetAttributeNameForLifetime(lifetime)
                });

                return;
            }

            abstractionType ??= typeInfo.ImplementedInterfaces.LastOrDefault(i => !typeInfo.ImplementedInterfaces.Any(parent => parent != i && i.IsAssignableFrom(parent)));

            if (abstractionType != null && !typeInfo.ImplementedInterfaces.Contains(abstractionType))
            {
                throw new Exception($"The type {implementationType.FullName} does not implement the abstraction type {abstractionType.FullName}.");
            }

            if (abstractionType == null)
            {
                throw new Exception($"No abstraction type found for {implementationType.FullName} that can implement tha service.");
            }

            switch (useTryAdd)
            {
                case true:
                    services.TryAdd(new ServiceDescriptor(abstractionType, implementationType, lifetime));
                    break;
                case false:
                    services.Add(new ServiceDescriptor(abstractionType, implementationType, lifetime));
                    break;
            }

            _registrationLog.Add(new ServiceRegistrationInfo
            {
                ServiceType = abstractionType,
                ImplementationType = implementationType,
                Lifetime = lifetime,
                RegistrationMethod = useTryAdd ? "TryAdd" : "Add",
                AttributeUsed = GetAttributeNameForLifetime(lifetime)
            });
        }

        private static void RegisterKeyedService(IServiceCollection services, Type abstractionType, Type implementationType, bool useTryAdd, string key, ServiceLifetime lifetime)
        {
#if NET8_0_OR_GREATER
            var typeInfo = implementationType.GetTypeInfo();

            if (!typeInfo.ImplementedInterfaces.Any())
            {
                if (useTryAdd)
                {
                    services.TryAdd(new ServiceDescriptor(implementationType, serviceKey: key, implementationType, lifetime: lifetime));
                }
                else
                {
                    services.Add(new ServiceDescriptor(implementationType, serviceKey: key, implementationType, lifetime: lifetime));
                }

                _registrationLog.Add(new ServiceRegistrationInfo
                {
                    ServiceType = implementationType,
                    ImplementationType = implementationType,
                    Lifetime = lifetime,
                    ServiceKey = key,
                    RegistrationMethod = useTryAdd ? "TryAdd" : "Add",
                    AttributeUsed = GetAttributeNameForLifetime(lifetime, isKeyed: true)
                });

                return;
            }

            abstractionType ??= typeInfo.ImplementedInterfaces.LastOrDefault(i => !typeInfo.ImplementedInterfaces.Any(parent => parent != i && i.IsAssignableFrom(parent)));

            if (abstractionType == null)
            {
                throw new Exception($"No abstraction type found for {implementationType.FullName} that can implement tha service.");
            }

            switch (useTryAdd)
            {
                case true:
                    services.TryAdd(new ServiceDescriptor(abstractionType, serviceKey: key, implementationType, lifetime));
                    break;
                case false:
                    services.Add(new ServiceDescriptor(abstractionType, serviceKey: key, implementationType, lifetime));
                    break;
            }

            _registrationLog.Add(new ServiceRegistrationInfo
            {
                ServiceType = abstractionType,
                ImplementationType = implementationType,
                Lifetime = lifetime,
                ServiceKey = key,
                RegistrationMethod = useTryAdd ? "TryAdd" : "Add",
                AttributeUsed = GetAttributeNameForLifetime(lifetime, isKeyed: true)
            });
#endif
        }

        private static string GetAttributeNameForLifetime(ServiceLifetime lifetime, bool isKeyed = false)
        {
            return (lifetime, isKeyed) switch
            {
                (ServiceLifetime.Singleton, false) => "RegisterAsSingleton",
                (ServiceLifetime.Singleton, true) => "RegisterAsSingletonKeyed",
                (ServiceLifetime.Scoped, false) => "RegisterAsScoped",
                (ServiceLifetime.Scoped, true) => "RegisterAsScopedKeyed",
                (ServiceLifetime.Transient, false) => "RegisterAsTransient",
                (ServiceLifetime.Transient, true) => "RegisterAsTransientKeyed",
                _ => "Unknown"
            };
        }

        private static void ApplyDecorators(IServiceCollection services, Assembly assembly)
        {
            foreach (var implementationType in assembly.DefinedTypes.Where(t => t.GetCustomAttributes<DecorateWithAttribute>().Any()))
            {
                // Get decorators and service registrations.
                var decoratorAttributes = implementationType.GetCustomAttributes<DecorateWithAttribute>()
                    .OrderByDescending(attr => attr.Order);

                foreach (var registration in _registrationLog.Where(r => r.ImplementationType == implementationType))
                {
                    if (registration.ServiceType == implementationType)
                    {
                        continue;
                    }

                    foreach (var attr in decoratorAttributes)
                    {
                        if (!registration.ServiceType.IsAssignableFrom(attr.DecoratorType))
                        {
                            throw new Exception($"Decorator type {attr.DecoratorType.FullName} does not implement service interface {registration.ServiceType.FullName}");
                        }

                        // Add decorator info and apply it
                        registration.Decorators.Add(new DecoratorInfo { DecoratorType = attr.DecoratorType, Order = attr.Order });

                        services.AddDecorator(registration.ServiceType, attr.DecoratorType);
                    }
                }
            }
        }
    }
}
