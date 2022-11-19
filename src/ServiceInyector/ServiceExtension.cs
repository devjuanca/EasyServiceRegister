using Microsoft.Extensions.DependencyInjection;
using ServiceInyector.Attributes;
using System.Reflection;

namespace ServiceInyector;

public static class ServiceExtension
{
    public static void AddServices(this IServiceCollection services, params Type[] handlerAssemblyMarkerTypes)
    {
        try
        {
            foreach (var markerType in handlerAssemblyMarkerTypes)
            {
                Assembly assembly = Assembly.GetAssembly(markerType) ?? throw new Exception("Assembly for this type does not exist");

                var singletonServicesToRegister = assembly.ExportedTypes.Where(a => a.GetCustomAttribute<RegisterAsSingletonAttribute>() is not null);

                var scopedServicesToRegister = assembly.ExportedTypes.Where(a => a.GetCustomAttribute<RegisterAsScopedAttribute>() is not null);

                var transientServicesToRegister = assembly.ExportedTypes.Where(a => a.GetCustomAttribute<RegisterAsTransientAttribute>() is not null);

                foreach (var implementationType in singletonServicesToRegister)
                {
                    var typeInfo = implementationType.GetTypeInfo();

                    if (!typeInfo.ImplementedInterfaces.Any())
                    {
                        services.AddSingleton(implementationType);
                    }
                    else
                    {
                        Type abstractionType = typeInfo.ImplementedInterfaces.ToArray()[0];

                        services.AddSingleton(abstractionType, implementationType);
                    }
                }

                foreach (var implementationType in scopedServicesToRegister)
                {
                    var typeInfo = implementationType.GetTypeInfo();

                    if (!typeInfo.ImplementedInterfaces.Any())
                    {
                        services.AddScoped(implementationType);
                    }
                    else
                    {
                        Type abstractionType = typeInfo.ImplementedInterfaces.ToArray().Last();

                        services.AddScoped(abstractionType, implementationType);
                    }
                }

                foreach (var implementationType in transientServicesToRegister)
                {
                    var typeInfo = implementationType.GetTypeInfo();

                    if (!typeInfo.ImplementedInterfaces.Any())
                    {
                        services.AddTransient(implementationType);
                    }
                    else
                    {
                        Type abstractionType = typeInfo.ImplementedInterfaces.ToArray()[0];

                        services.AddTransient(abstractionType, implementationType);
                    }
                }
            }
        }
        catch
        {
            throw;
        }



    }
}
