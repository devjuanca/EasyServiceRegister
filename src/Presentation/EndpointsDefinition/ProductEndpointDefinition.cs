using Application.Common.Dto;
using Application.Cqrs.Products.Command;
using Application.Cqrs.Products.Query;
using Application.Interfaces.ProductServices;
using Infrastructure.Services.ProductServices;
using MediatR;
using Presentation.EndpointsRegistration;

namespace Presentation.EndpointsDefinition;

using static Presentation.EndpointsCommon.EndpointTags;
public class ProductEndpointDefinition : IEndpointDefinition
{
    public void DefineEndpoints(WebApplication app)
    {
        app.MapPost("/api/addProduct", async (AddProductCommand command, ISender sender) =>
        {
            return Results.Created(string.Empty, await sender.Send(command));

        })
            .Produces<Unit>(StatusCodes.Status201Created)
            .WithTags(ProductEndpoints);


        app.MapGet("/api/getProducts", async (ISender sender) =>
        {
            return await sender.Send(new ProductsQuery());

        })
            .Produces<List<ProductDto>>(StatusCodes.Status200OK)
            .WithTags(ProductEndpoints);
    }

    public void DefineServices(IServiceCollection services, IConfiguration configuration)
    { }
}

