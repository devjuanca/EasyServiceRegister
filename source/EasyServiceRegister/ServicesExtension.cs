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
        /// Clears the registration log. Useful for test scenarios where AddServices is called multiple times.
        /// </summary>
        public static void ClearRegistrationLog()
        {
            _registrationLog.Clear();
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
            foreach (var markerType in handlerAssemblyMarkerTypes)
            {
                Assembly assembly = Assembly.GetAssembly(markerType) ?? throw new Exception("Assembly for this type does not exist");
                RegisterAllFromAssembly(services, assembly, filter: null);
            }

            return services;
        }

        /// <summary>
        /// Scans the provided assemblies and registers services
        /// based on custom attributes such as RegisterAsSingleton, RegisterAsScoped, RegisterAsTransient,
        /// and their keyed variants.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="assemblies">The assemblies to scan for service registrations.</param>
        /// <returns>The IServiceCollection for chaining.</returns>
        public static IServiceCollection AddServices(this IServiceCollection services, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAllFromAssembly(services, assembly, filter: null);
            }
            return services;
        }

        /// <summary>
        /// Scans the assemblies containing the specified marker types and registers services,
        /// applying the given filter to control which types are registered.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="filter">A predicate to filter which types should be registered. Types for which the predicate returns false are skipped.</param>
        /// <param name="handlerAssemblyMarkerTypes">Types used to locate assemblies to scan for service registrations.</param>
        /// <returns>The IServiceCollection for chaining.</returns>
        public static IServiceCollection AddServices(this IServiceCollection services, Func<TypeInfo, bool> filter, params Type[] handlerAssemblyMarkerTypes)
        {
            foreach (var markerType in handlerAssemblyMarkerTypes)
            {
                Assembly assembly = Assembly.GetAssembly(markerType) ?? throw new Exception("Assembly for this type does not exist");
                RegisterAllFromAssembly(services, assembly, filter);
            }

            return services;
        }

        /// <summary>
        /// Scans the provided assemblies and registers services,
        /// applying the given filter to control which types are registered.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="filter">A predicate to filter which types should be registered. Types for which the predicate returns false are skipped.</param>
        /// <param name="assemblies">The assemblies to scan for service registrations.</param>
        /// <returns>The IServiceCollection for chaining.</returns>
        public static IServiceCollection AddServices(this IServiceCollection services, Func<TypeInfo, bool> filter, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAllFromAssembly(services, assembly, filter);
            }
            return services;
        }

        /// <summary>
        /// Performs a single pass over the assembly's defined types, collecting and processing
        /// all registration attributes in one iteration instead of scanning the assembly multiple times.
        /// </summary>
        private static void RegisterAllFromAssembly(IServiceCollection services, Assembly assembly, Func<TypeInfo, bool> filter)
        {
            foreach (var typeInfo in assembly.DefinedTypes)
            {
                if (filter != null && !filter(typeInfo))
                    continue;

                // Read each attribute type once per type (avoids redundant reflection)
                var singletonAttr = typeInfo.GetCustomAttribute<RegisterAsSingletonAttribute>();
                var scopedAttr = typeInfo.GetCustomAttribute<RegisterAsScopedAttribute>();
                var transientAttr = typeInfo.GetCustomAttribute<RegisterAsTransientAttribute>();

                if (singletonAttr != null)
                {
                    RegisterService(services, singletonAttr.ServiceInterface, typeInfo, singletonAttr.UseTryAddSingleton, ServiceLifetime.Singleton, singletonAttr.RegisterAsAllInterfaces);
                }

                if (scopedAttr != null)
                {
                    RegisterService(services, scopedAttr.ServiceInterface, typeInfo, scopedAttr.UseTryAddScoped, ServiceLifetime.Scoped, scopedAttr.RegisterAsAllInterfaces);
                }

                if (transientAttr != null)
                {
                    RegisterService(services, transientAttr.ServiceInterface, typeInfo, transientAttr.UseTryAddTransient, ServiceLifetime.Transient, transientAttr.RegisterAsAllInterfaces);
                }

#if NET8_0_OR_GREATER
                var singletonKeyedAttr = typeInfo.GetCustomAttribute<RegisterAsSingletonKeyedAttribute>();
                var scopedKeyedAttr = typeInfo.GetCustomAttribute<RegisterAsScopedKeyedAttribute>();
                var transientKeyedAttr = typeInfo.GetCustomAttribute<RegisterAsTransientKeyedAttribute>();

                if (singletonKeyedAttr != null)
                {
                    RegisterKeyedService(services, singletonKeyedAttr.ServiceInterface, typeInfo, singletonKeyedAttr.UseTryAddSingleton, singletonKeyedAttr.Key, ServiceLifetime.Singleton, singletonKeyedAttr.RegisterAsAllInterfaces);
                }

                if (scopedKeyedAttr != null)
                {
                    RegisterKeyedService(services, scopedKeyedAttr.ServiceInterface, typeInfo, scopedKeyedAttr.UseTryAddScoped, scopedKeyedAttr.Key, ServiceLifetime.Scoped, scopedKeyedAttr.RegisterAsAllInterfaces);
                }

                if (transientKeyedAttr != null)
                {
                    RegisterKeyedService(services, transientKeyedAttr.ServiceInterface, typeInfo, transientKeyedAttr.UseTryAddTransient, transientKeyedAttr.Key, ServiceLifetime.Transient, transientKeyedAttr.RegisterAsAllInterfaces);
                }
#endif

                // Collect decorator attributes for this type
                var decoratorAttributes = typeInfo.GetCustomAttributes<DecorateWithAttribute>().ToList();
                if (decoratorAttributes.Count > 0)
                {
                    ApplyDecoratorsForType(services, typeInfo, decoratorAttributes);
                }
            }
        }

        private static void RegisterService(IServiceCollection services, Type abstractionType, TypeInfo implementationType, bool useTryAdd, ServiceLifetime lifetime, bool registerAsAllInterfaces)
        {
            // Handle registerAsAllInterfaces mode
            if (registerAsAllInterfaces)
            {
                RegisterAsAllInterfacesInternal(services, implementationType, useTryAdd, lifetime);
                return;
            }

            // Handle open generic registrations
            if (implementationType.IsGenericTypeDefinition)
            {
                var impl = implementationType.AsType();

                // If abstraction type not provided, try to infer matching generic interface definition
                if (abstractionType == null)
                {
                    var match = implementationType.ImplementedInterfaces
                        .Select(i => i.IsGenericType ? i.GetGenericTypeDefinition() : i)
                        .LastOrDefault(i => !implementationType.ImplementedInterfaces.Any(parent => parent != i && (i.IsAssignableFrom(parent.IsGenericType ? parent.GetGenericTypeDefinition() : parent))));

                    abstractionType = match;
                }

                // Self registration if no interfaces
                if (!implementationType.ImplementedInterfaces.Any() && abstractionType == null)
                {
                    var descriptor = new ServiceDescriptor(impl, impl, lifetime);

                    if (useTryAdd)
                    {
                        services.TryAdd(descriptor);
                    }
                    else
                    {
                        services.Add(descriptor);
                    }

                    _registrationLog.Add(new ServiceRegistrationInfo
                    {
                        ServiceType = impl,
                        ImplementationType = impl,
                        Lifetime = lifetime,
                        RegistrationMethod = useTryAdd ? "TryAdd" : "Add",
                        AttributeUsed = GetAttributeNameForLifetime(lifetime)
                    });
                    return;
                }

                // Normalize abstraction to generic type definition if needed
                if (abstractionType != null)
                {
                    abstractionType = abstractionType.IsGenericType ? abstractionType.GetGenericTypeDefinition() : abstractionType;

                    // Validate implementation actually implements abstraction generic definition
                    var implementedGenericDefs = implementationType.ImplementedInterfaces
                        .Select(i => i.IsGenericType ? i.GetGenericTypeDefinition() : i)
                        .ToHashSet();

                    if (!implementedGenericDefs.Contains(abstractionType))
                    {
                        throw new InvalidOperationException($"The type {implementationType.FullName} does not implement the abstraction type {abstractionType.FullName}.");
                    }

                    var descriptor = new ServiceDescriptor(abstractionType, impl, lifetime);

                    if (useTryAdd)
                    {
                        services.TryAdd(descriptor);
                    }
                    else
                    {
                        services.Add(descriptor);
                    }

                    _registrationLog.Add(new ServiceRegistrationInfo
                    {
                        ServiceType = abstractionType,
                        ImplementationType = impl,
                        Lifetime = lifetime,
                        RegistrationMethod = useTryAdd ? "TryAdd" : "Add",
                        AttributeUsed = GetAttributeNameForLifetime(lifetime)
                    });
                    return;
                }

                throw new InvalidOperationException($"No abstraction type found for {implementationType.FullName} that can implement the service.");
            }

            if (!implementationType.ImplementedInterfaces.Any())
            {
                if (useTryAdd)
                {
                    services.TryAdd(new ServiceDescriptor(implementationType.AsType(), implementationType.AsType(), lifetime: lifetime));
                }
                else
                {
                    services.Add(new ServiceDescriptor(implementationType.AsType(), implementationType.AsType(), lifetime: lifetime));
                }

                _registrationLog.Add(new ServiceRegistrationInfo
                {
                    ServiceType = implementationType.AsType(),
                    ImplementationType = implementationType.AsType(),
                    Lifetime = lifetime,
                    RegistrationMethod = useTryAdd ? "TryAdd" : "Add",
                    AttributeUsed = GetAttributeNameForLifetime(lifetime)
                });

                return;
            }

            abstractionType ??= implementationType.ImplementedInterfaces.LastOrDefault(i => !implementationType.ImplementedInterfaces.Any(parent => parent != i && i.IsAssignableFrom(parent)));

            if (abstractionType != null && !implementationType.ImplementedInterfaces.Contains(abstractionType))
            {
                throw new InvalidOperationException($"The type {implementationType.FullName} does not implement the abstraction type {abstractionType.FullName}.");
            }

            if (abstractionType == null)
            {
                throw new InvalidOperationException($"No abstraction type found for {implementationType.FullName} that can implement the service.");
            }

            switch (useTryAdd)
            {
                case true:
                    services.TryAdd(new ServiceDescriptor(abstractionType, implementationType.AsType(), lifetime));
                    break;
                case false:
                    services.Add(new ServiceDescriptor(abstractionType, implementationType.AsType(), lifetime));
                    break;
            }

            _registrationLog.Add(new ServiceRegistrationInfo
            {
                ServiceType = abstractionType,
                ImplementationType = implementationType.AsType(),
                Lifetime = lifetime,
                RegistrationMethod = useTryAdd ? "TryAdd" : "Add",
                AttributeUsed = GetAttributeNameForLifetime(lifetime)
            });
        }

        private static void RegisterKeyedService(IServiceCollection services, Type abstractionType, Type implementationType, bool useTryAdd, object key, ServiceLifetime lifetime, bool registerAsAllInterfaces)
        {
#if NET8_0_OR_GREATER
            // Handle registerAsAllInterfaces mode
            if (registerAsAllInterfaces)
            {
                RegisterAsAllInterfacesKeyedInternal(services, implementationType, useTryAdd, key, lifetime);
                return;
            }

            var typeInfo = implementationType.GetTypeInfo();

            // Handle open generic registrations for keyed services
            if (typeInfo.IsGenericTypeDefinition)
            {
                var impl = implementationType;

                if (abstractionType == null)
                {
                    var match = typeInfo.ImplementedInterfaces
                        .Select(i => i.IsGenericType ? i.GetGenericTypeDefinition() : i)
                        .LastOrDefault(i => !typeInfo.ImplementedInterfaces.Any(parent => parent != i && (i.IsAssignableFrom(parent.IsGenericType ? parent.GetGenericTypeDefinition() : parent))));

                    abstractionType = match;
                }

                if (!typeInfo.ImplementedInterfaces.Any() && abstractionType == null)
                {
                    var descriptor = new ServiceDescriptor(impl, serviceKey: key, impl, lifetime: lifetime);

                    if (useTryAdd)
                    {
                        services.TryAdd(descriptor);
                    }
                    else
                    {
                        services.Add(descriptor);
                    }

                    _registrationLog.Add(new ServiceRegistrationInfo
                    {
                        ServiceType = impl,
                        ImplementationType = impl,
                        Lifetime = lifetime,
                        ServiceKey = key,
                        RegistrationMethod = useTryAdd ? "TryAdd" : "Add",
                        AttributeUsed = GetAttributeNameForLifetime(lifetime, isKeyed: true)
                    });
                    return;
                }

                if (abstractionType != null)
                {
                    abstractionType = abstractionType.IsGenericType ? abstractionType.GetGenericTypeDefinition() : abstractionType;

                    var implementedGenericDefs = typeInfo.ImplementedInterfaces
                        .Select(i => i.IsGenericType ? i.GetGenericTypeDefinition() : i)
                        .ToHashSet();

                    if (!implementedGenericDefs.Contains(abstractionType))
                    {
                        throw new InvalidOperationException($"No abstraction type found for {implementationType.FullName} that can implement the service.");
                    }

                    var descriptor = new ServiceDescriptor(abstractionType, serviceKey: key, impl, lifetime);

                    if (useTryAdd)
                    {
                        services.TryAdd(descriptor);
                    }
                    else
                    {
                        services.Add(descriptor);
                    }

                    _registrationLog.Add(new ServiceRegistrationInfo
                    {
                        ServiceType = abstractionType,
                        ImplementationType = impl,
                        Lifetime = lifetime,
                        ServiceKey = key,
                        RegistrationMethod = useTryAdd ? "TryAdd" : "Add",
                        AttributeUsed = GetAttributeNameForLifetime(lifetime, isKeyed: true)
                    });
                    return;
                }

                throw new InvalidOperationException($"No abstraction type found for {implementationType.FullName} that can implement the service.");
            }

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
                throw new InvalidOperationException($"No abstraction type found for {implementationType.FullName} that can implement the service.");
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

        private static void RegisterAsAllInterfacesInternal(IServiceCollection services, TypeInfo implementationType, bool useTryAdd, ServiceLifetime lifetime)
        {
            var implType = implementationType.AsType();
            var interfaces = implementationType.ImplementedInterfaces.ToList();

            if (interfaces.Count == 0)
            {
                throw new InvalidOperationException($"RegisterAsAllInterfaces was specified for {implementationType.FullName} but it does not implement any interfaces.");
            }

            foreach (var iface in interfaces)
            {
                var descriptor = new ServiceDescriptor(iface, implType, lifetime);

                if (useTryAdd)
                {
                    services.TryAdd(descriptor);
                }
                else
                {
                    services.Add(descriptor);
                }

                _registrationLog.Add(new ServiceRegistrationInfo
                {
                    ServiceType = iface,
                    ImplementationType = implType,
                    Lifetime = lifetime,
                    RegistrationMethod = useTryAdd ? "TryAdd" : "Add",
                    AttributeUsed = GetAttributeNameForLifetime(lifetime)
                });
            }
        }

#if NET8_0_OR_GREATER
        private static void RegisterAsAllInterfacesKeyedInternal(IServiceCollection services, Type implementationType, bool useTryAdd, object key, ServiceLifetime lifetime)
        {
            var typeInfo = implementationType.GetTypeInfo();
            var interfaces = typeInfo.ImplementedInterfaces.ToList();

            if (interfaces.Count == 0)
            {
                throw new InvalidOperationException($"RegisterAsAllInterfaces was specified for {implementationType.FullName} but it does not implement any interfaces.");
            }

            foreach (var iface in interfaces)
            {
                var descriptor = new ServiceDescriptor(iface, serviceKey: key, implementationType, lifetime);

                if (useTryAdd)
                {
                    services.TryAdd(descriptor);
                }
                else
                {
                    services.Add(descriptor);
                }

                _registrationLog.Add(new ServiceRegistrationInfo
                {
                    ServiceType = iface,
                    ImplementationType = implementationType,
                    Lifetime = lifetime,
                    ServiceKey = key,
                    RegistrationMethod = useTryAdd ? "TryAdd" : "Add",
                    AttributeUsed = GetAttributeNameForLifetime(lifetime, isKeyed: true)
                });
            }
        }
#endif

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

        private static void ApplyDecoratorsForType(IServiceCollection services, TypeInfo implementationType, List<DecorateWithAttribute> decoratorAttributes)
        {
            var orderedDecorators = decoratorAttributes.OrderByDescending(attr => attr.Order);

            foreach (var registration in _registrationLog.Where(r => r.ImplementationType == implementationType.AsType()))
            {
                if (registration.ServiceType == implementationType.AsType())
                {
                    continue;
                }

                foreach (var attr in orderedDecorators)
                {
                    if (!registration.ServiceType.IsAssignableFrom(attr.DecoratorType))
                    {
                        throw new InvalidOperationException($"Decorator type {attr.DecoratorType.FullName} does not implement service interface {registration.ServiceType.FullName}");
                    }

                    // Add decorator info and apply it
                    registration.Decorators.Add(new DecoratorInfo { DecoratorType = attr.DecoratorType, Order = attr.Order });

                    services.AddDecorator(registration.ServiceType, attr.DecoratorType);
                }
            }
        }
    }
}
