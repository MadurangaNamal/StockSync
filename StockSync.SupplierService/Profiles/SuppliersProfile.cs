using AutoMapper;
using StockSync.SupplierService.Entities;
using StockSync.SupplierService.Models;

namespace StockSync.SupplierService.Profiles;

public class SuppliersProfile : Profile
{
    public SuppliersProfile()
    {
        CreateMap<Supplier, SupplierDto>();
        CreateMap<SupplierManipulationDto, Supplier>();
    }
}
