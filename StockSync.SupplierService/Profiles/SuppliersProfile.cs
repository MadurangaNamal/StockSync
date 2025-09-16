using AutoMapper;
using StockSync.SupplierService.Entities;
using StockSync.SupplierService.Models;

namespace StockSync.SupplierService.Profiles;

public class SuppliersProfile : Profile
{
    public SuppliersProfile()
    {
        CreateMap<Supplier, SupplierDto>()
           .ForMember(dest => dest.Items, opt => opt.Ignore());

        CreateMap<SupplierManipulationDto, Supplier>();
    }
}
