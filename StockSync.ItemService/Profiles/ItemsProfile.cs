using AutoMapper;
using StockSync.ItemService.Entities;
using StockSync.ItemService.Models;

namespace StockSync.ItemService.Profiles;

public class ItemsProfile : Profile
{
    public ItemsProfile()
    {
        CreateMap<Item, ItemDto>();
        CreateMap<ItemManipulationDto, Item>();
    }
}
