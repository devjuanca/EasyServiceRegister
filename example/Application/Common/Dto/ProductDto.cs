using Application.Mappings;
using Domain.Entities;

namespace Application.Common.Dto;

public class ProductDto : IMapFrom<Product>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public static void Mapping(MappingProfile profile)
    {
        profile.CreateMap<Product, ProductDto>().ReverseMap();
    }
}

