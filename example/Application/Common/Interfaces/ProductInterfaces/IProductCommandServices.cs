using Application.Common.Dto;
using MediatR;

namespace Application.Interfaces.ProductServices
{
    public interface IProductCommandServices
    {
        Task<Unit> AddNewProduct(ProductDto product, CancellationToken cancellationToken);
    }
}
