using StockSync.ItemService.Entities;

namespace StockSync.ItemService.Infrastructure;

public interface IItemServiceRepository
{
    Task<Item?> GetItemByIdAsync(string id);
    Task<IEnumerable<Item>> GetItemsAsync(string? itemIds);
    Task<Item> CreateItemAsync(Item item);
    Task<Item> UpdateItemAsync(string id, Item item);
    Task<bool> DeleteItemAsync(string id);
    Task<bool> ItemExistsAsync(string id);
}
