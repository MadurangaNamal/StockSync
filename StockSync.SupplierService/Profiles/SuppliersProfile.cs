using AutoMapper;
using StockSync.SupplierService.Entities;
using StockSync.SupplierService.Infrastructure;
using StockSync.SupplierService.Models;

namespace StockSync.SupplierService.Profiles;

public class SuppliersProfile : Profile
{
    public SuppliersProfile(ICacheService cacheService)
    {
        CreateMap<Supplier, SupplierDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom((src, dest) =>
                (src.Items != null ? src.Items.Select(itemId => cacheService.GetItemDto(itemId)).Where(item => item != null).ToList() : [])));

        CreateMap<SupplierManipulationDto, Supplier>();
    }
}
