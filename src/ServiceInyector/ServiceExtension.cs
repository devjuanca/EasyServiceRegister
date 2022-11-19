using Microsoft.Extensions.DependencyInjection;
using EasyServiceRegister.Attributes;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyServiceRegister;

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

                var singletonServicesToRegister = assembly.DefinedTypes.Where(a => a.GetCustomAttribute<RegisterAsSingletonAttribute>() is not null);

                var scopedServicesToRegister = assembly.DefinedTypes.Where(a => a.GetCustomAttribute<RegisterAsScopedAttribute>() is not null);

                var transientServicesToRegister = assembly.DefinedTypes.Where(a => a.GetCustomAttribute<RegisterAsTransientAttribute>() is not null);

                foreach (var implementationType in singletonServicesToRegister)
                {
                    var typeInfo = implementationType.GetTypeInfo();

                    var registerBehaviour = typeInfo.GetCustomAttribute<RegisterAsSingletonAttribute>()?.UseTryAddSingleton ?? true;

                    if (!typeInfo.ImplementedInterfaces.Any())
                    {
                        if (registerBehaviour)
                            services.AddSingleton(implementationType);
                        else
                            services.TryAddSingleton(implementationType);
                    }
                    else
                    {
                        Type abstractionType = typeInfo.ImplementedInterfaces.ToArray()[0];

                        if (registerBehaviour)
                            services.AddSingleton(abstractionType, implementationType);
                        else
                            services.AddSingleton(abstractionType, implementationType);
                    }
                }

                foreach (var implementationType in scopedServicesToRegister)
                {
                    var typeInfo = implementationType.GetTypeInfo();

                    var registerBehaviour = typeInfo.GetCustomAttribute<RegisterAsScopedAttribute>()?.UseTryAddScoped ?? true;

                    if (!typeInfo.ImplementedInterfaces.Any())
                    {
                        if (!registerBehaviour)
                            services.AddScoped(implementationType);
                        else
                            services.TryAddScoped(implementationType);
                    }
                    else
                    {
                        Type abstractionType = typeInfo.ImplementedInterfaces.ToArray().Last();

                        if (!registerBehaviour)
                            services.AddScoped(abstractionType, implementationType);
                        else
                            services.TryAddScoped(abstractionType, implementationType);
                    }
                }

                foreach (var implementationType in transientServicesToRegister)
                {
                    var typeInfo = implementationType.GetTypeInfo();

                    var registerBehaviour = typeInfo.GetCustomAttribute<RegisterAsTransientAttribute>()?.UseTryAddTransient ?? false;

                    if (!typeInfo.ImplementedInterfaces.Any())
                    {
                        if (!registerBehaviour)
                            services.AddTransient(implementationType);
                        else
                            services.TryAddTransient(implementationType);
                    }
                    else
                    {
                        Type abstractionType = typeInfo.ImplementedInterfaces.ToArray()[0];

                        if (!registerBehaviour)
                            services.AddTransient(abstractionType, implementationType);
                        else
                            services.TryAddTransient(abstractionType, implementationType);
                    }
                }
            }

            return services;
        }
        catch
        {
            throw;
        }



    }
}
