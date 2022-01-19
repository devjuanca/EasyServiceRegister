using Microsoft.Extensions.DependencyInjection;
using ServiceInyector.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceInyector
{
    public static class ServiceExtension
    {
        public static void AddServices(this IServiceCollection services, List<string> projectsAssembly)
        {
            try
            {
                var servicesToRegister = new List<IServiceRegistrationType>();
                List<Assembly> assemblies = new();

                foreach (var assemblyName in projectsAssembly)
                {
                    Assembly assembly = Assembly.LoadFile(Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)!, $"{assemblyName}.dll"));

                    var temp = assembly.ExportedTypes
                             .Where(x => typeof(IServiceRegistrationType).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

                    foreach (var implementationType in temp)
                    {
                        var typeInfo = implementationType.GetTypeInfo();

                        Type abstractionType = typeInfo.ImplementedInterfaces.ToArray()[0];

                        Type registerType = typeInfo.ImplementedInterfaces.ToArray()[1];

                        switch (registerType.Name)
                        {
                            case "IRegisterAsScoped":
                                {
                                    services.AddScoped(abstractionType, implementationType);
                                }
                                break;
                            case "IRegisterAsSingleton":
                                {
                                    services.AddSingleton(abstractionType, implementationType);
                                }
                                break;
                            case "IRegisterAsTranscient":
                                {
                                    services.AddTransient(abstractionType, implementationType);
                                }
                                break;
                            default:
                                {
                                    services.AddScoped(abstractionType, implementationType);
                                }
                                break;
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
}
