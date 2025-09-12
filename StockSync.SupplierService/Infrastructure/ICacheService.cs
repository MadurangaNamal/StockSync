using StockSync.SupplierService.Models;

namespace StockSync.SupplierService.Infrastructure;

public interface ICacheService
{
    void SetItemDto(string itemId, ItemDto itemDto, TimeSpan? expiry = null);
    ItemDto? GetItemDto(string itemId);
    void SetAllItemDtos(Dictionary<string, ItemDto> items, TimeSpan? expiry = null);
}
