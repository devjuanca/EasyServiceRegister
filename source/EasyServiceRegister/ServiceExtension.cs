using EasyServiceRegister.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace EasyServiceRegister
{

    public static class ServiceExtension
    {
        /// <summary>
        /// Extension method to register all services marked with attributes [RegisterAsSingleton], [RegisterAsScoped] or [RegisterAsTransient].
        /// </summary>
        /// <param name="services"></param>
        /// <param name="handlerAssemblyMarkerTypes"></param>
        /// <returns></returns>
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
                }

                return services;
            }
            catch
            {
                throw;
            }
        }

        public static IServiceCollection AddServices(this IServiceCollection services, params Assembly[] assemblies)
        {
            try
            {
                foreach (var assembly in assemblies)
                {
                    RegisterSingletonServices(services, assembly);
                    RegisterScopedServices(services, assembly);
                    RegisterTransientServices(services, assembly);
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

        private static void RegisterService(IServiceCollection services, Type implementationType, bool useTryAdd, ServiceLifetime lifetime)
        {
            var typeInfo = implementationType.GetTypeInfo();

            if (!typeInfo.ImplementedInterfaces.Any())
            {
                services.Add(new ServiceDescriptor(implementationType, lifetime));
                return;
            }

            Type abstractionType = typeInfo.ImplementedInterfaces.Last();

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
    }
}
