using System.Reflection;

namespace Presentation.EndpointsRegistration;

public static class EndpointDefinitionExtentions
{
    public static void AddEndpointDefinitions(this IServiceCollection services, IConfiguration configuration)
    {
        var endpointDefinitions = new List<IEndpointDefinition>();


        endpointDefinitions.AddRange(
                    Assembly.GetExecutingAssembly().ExportedTypes
                   .Where(x => typeof(IEndpointDefinition).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                   .Select(Activator.CreateInstance).Cast<IEndpointDefinition>());

        foreach (var endpointDefinition in endpointDefinitions)
        {
            endpointDefinition.DefineServices(services, configuration);
        }

        services.AddSingleton(endpointDefinitions as IReadOnlyCollection<IEndpointDefinition>);



    }

    public static void UseEndpointDefinitions(this WebApplication app)
    {
        var definitions = app.Services.GetRequiredService<IReadOnlyCollection<IEndpointDefinition>>();

        foreach (var endpointDefinition in definitions)
        {
            endpointDefinition.DefineEndpoints(app);
        }
    }
}

