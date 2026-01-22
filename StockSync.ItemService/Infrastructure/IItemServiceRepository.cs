using StockSync.ItemService.Entities;
using StockSync.Shared.Models;

namespace StockSync.ItemService.Infrastructure;

public interface IItemServiceRepository
{
    Task<PagedResult<Item>> GetItemsAsync(string? itemIds, PaginationParams pagination);
    Task<Item?> GetItemByIdAsync(string id);
    Task<Item> CreateItemAsync(Item item);
    Task<Item> UpdateItemAsync(string id, Item item);
    Task<bool> DeleteItemAsync(string id);
    Task<bool> ItemExistsAsync(string id);
}
