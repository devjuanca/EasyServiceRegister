using Application.Common.Dto;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;

namespace Application.Cqrs.Products.Query;

public class ProductsQuery : IRequest<List<ProductDto>>
{ }

public class ProductQueryHandler : IRequestHandler<ProductsQuery, List<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ProductQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ProductDto>> Handle(ProductsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Products.ProjectTo<ProductDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
    }
}

