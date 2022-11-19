using Application.Common.Dto;
using MediatR;
using Application.Interfaces.ProductServices;
using Microsoft.Extensions.Logging;

namespace Application.Cqrs.Products.Command;

public class AddProductCommand : ProductDto, IRequest<Unit>
{

}

public class AddProductCommandHandler : IRequestHandler<AddProductCommand, Unit>
{
    private readonly IProductCommandServices _productService;
    private readonly ILogger<AddProductCommandHandler> _logger;

    public AddProductCommandHandler(IProductCommandServices productService, ILogger<AddProductCommandHandler> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public async Task<Unit> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _productService.AddNewProduct(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Some error occurred creating product {request.Name} ");
            throw;
        }
    }
}

