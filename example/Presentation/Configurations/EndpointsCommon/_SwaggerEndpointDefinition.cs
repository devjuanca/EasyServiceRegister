using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using Presentation.EndpointsRegistration;

namespace Presentation.EndpointsCommon;

public class SwaggerEndpointDefinition : IEndpointDefinition
{
    public void DefineEndpoints(WebApplication app)
    {
        var configuration = app.Configuration;

        app.UseOpenApi();
        app.UseSwaggerUi3(settings =>
        {
            settings.DocExpansion = "list";
            settings.DocumentTitle = "CleanArchitecture.Api Swagger";
            settings.Path = "/swagger";
        });
    }

    public void DefineServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();

        services.AddOpenApiDocument(configure =>
        {
            configure.Title = "CleanArchitecture.Api";
            configure.Version = "v1";
        });

    }
}

