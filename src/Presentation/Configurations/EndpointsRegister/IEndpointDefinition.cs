
namespace Presentation.EndpointsRegistration;

public interface IEndpointDefinition
{
    void DefineEndpoints(WebApplication app);
    void DefineServices(IServiceCollection services, IConfiguration configuration);
}


