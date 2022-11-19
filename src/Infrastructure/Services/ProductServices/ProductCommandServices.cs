using Application.Common.Dto;
using Application.Interfaces;
using Application.Interfaces.ProductServices;
using AutoMapper;
using Domain.Entities;
using MediatR;
using ServiceInyector.Attributes;

namespace Infrastructure.Services.ProductServices;

[RegisterAsScoped]
internal class ProductCommandServices : IProductCommandServices
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ProductCommandServices(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Unit> AddNewProduct(ProductDto product, CancellationToken cancellationToken)
    {
        try
        {
            _context.Products.Add(_mapper.Map<Product>(product));

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
        catch { throw; }
    }
}

