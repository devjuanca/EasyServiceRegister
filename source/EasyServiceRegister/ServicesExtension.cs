using EasyServiceRegister.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace EasyServiceRegister
{

    public static class ServicesExtension
    {
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
                var registerBehaviour = typeInfo.GetCustomAttribute<RegisterAsSingletonAttribute>()?.UseTryAddSingleton ?? true;
                RegisterService(services, implementationType, registerBehaviour, ServiceLifetime.Singleton);
            }
        }

        private static void RegisterScopedServices(IServiceCollection services, Assembly assembly)
        {
            var scopedServicesToRegister = assembly.DefinedTypes.Where(a => a.GetCustomAttribute<RegisterAsScopedAttribute>() != null);

            foreach (var implementationType in scopedServicesToRegister)
            {
                var typeInfo = implementationType.GetTypeInfo();
                var registerBehaviour = typeInfo.GetCustomAttribute<RegisterAsScopedAttribute>()?.UseTryAddScoped ?? true;
                RegisterService(services, implementationType, registerBehaviour, ServiceLifetime.Scoped);
            }
        }

        private static void RegisterTransientServices(IServiceCollection services, Assembly assembly)
        {
            var transientServicesToRegister = assembly.DefinedTypes.Where(a => a.GetCustomAttribute<RegisterAsTransientAttribute>() != null);

            foreach (var implementationType in transientServicesToRegister)
            {
                var typeInfo = implementationType.GetTypeInfo();
                var registerBehaviour = typeInfo.GetCustomAttribute<RegisterAsTransientAttribute>()?.UseTryAddTransient ?? false;
                RegisterService(services, implementationType, registerBehaviour, ServiceLifetime.Transient);
            }
        }

        private static void RegisterKeyedScopedService(IServiceCollection services, Assembly assembly)
        {
            var scopedServicesToRegister = assembly.DefinedTypes.Where(a => a.GetCustomAttribute<RegisterAsScopedKeyedAttribute>() != null);

            foreach (var implementationType in scopedServicesToRegister)
            {
                var typeInfo = implementationType.GetTypeInfo();
                var registerBehaviour = typeInfo.GetCustomAttribute<RegisterAsScopedKeyedAttribute>()?.UseTryAddScoped ?? false;
                var key = typeInfo.GetCustomAttribute<RegisterAsScopedKeyedAttribute>().Key;
                RegisterKeyedService(services, implementationType, registerBehaviour, key, ServiceLifetime.Scoped);
            }
        }

        private static void RegisterKeyedSingletonService(IServiceCollection services, Assembly assembly)
        {
            var singletonServicesToRegister = assembly.DefinedTypes.Where(a => a.GetCustomAttribute<RegisterAsSingletonKeyedAttribute>() != null);

            foreach (var implementationType in singletonServicesToRegister)
            {
                var typeInfo = implementationType.GetTypeInfo();
                var registerBehaviour = typeInfo.GetCustomAttribute<RegisterAsSingletonKeyedAttribute>()?.UseTryAddSingleton ?? true;
                var key = typeInfo.GetCustomAttribute<RegisterAsSingletonKeyedAttribute>().Key;
                RegisterKeyedService(services, implementationType, registerBehaviour, key, ServiceLifetime.Singleton);
            }
        }

        private static void RegisterKeyedTransientService(IServiceCollection services, Assembly assembly)
        {
            var transientServicesToRegister = assembly.DefinedTypes.Where(a => a.GetCustomAttribute<RegisterAsTransientKeyedAttribute>() != null);

            foreach (var implementationType in transientServicesToRegister)
            {
                var typeInfo = implementationType.GetTypeInfo();
                var registerBehaviour = typeInfo.GetCustomAttribute<RegisterAsTransientKeyedAttribute>()?.UseTryAddTransient ?? false;
                var key = typeInfo.GetCustomAttribute<RegisterAsTransientKeyedAttribute>().Key;
                RegisterKeyedService(services, implementationType, registerBehaviour, key, ServiceLifetime.Transient);
            }
        }

        private static void RegisterService(IServiceCollection services, Type implementationType, bool useTryAdd, ServiceLifetime lifetime)
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
                return;
            }

            Type abstractionType = typeInfo.ImplementedInterfaces.LastOrDefault(i => !typeInfo.ImplementedInterfaces.Any(parent => parent != i && i.IsAssignableFrom(parent)));

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
        }

        private static void RegisterKeyedService(IServiceCollection services, Type implementationType, bool useTryAdd, string key, ServiceLifetime lifetime)
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

                return;
            }

            Type abstractionType = typeInfo.ImplementedInterfaces.LastOrDefault(i => !typeInfo.ImplementedInterfaces.Any(parent => parent != i && i.IsAssignableFrom(parent)));

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
#endif
        }
    }
}
