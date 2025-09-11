using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using StockSync.ItemService.Data;
using StockSync.ItemService.Entities;

namespace StockSync.ItemService.Infrastructure;

public class ItemServiceRepository : IItemServiceRepository
{
    private readonly ItemServiceDBContext _dbContext;

    public ItemServiceRepository(ItemServiceDBContext context)
    {
        _dbContext = context;
    }

    public async Task<Item?> GetItemByIdAsync(string id)
    {
        return await _dbContext.Items.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
    }

    public async Task<IEnumerable<Item>> GetAllItemsAsync(string? itemIds)
    {
        if (!string.IsNullOrEmpty(itemIds))
        {
            var ids = itemIds.Split(',').Select(id => id.Trim()).ToList();
            return await _dbContext.Items.AsNoTracking().Where(item => ids.Contains(item.Id)).ToListAsync();
        }

        return await _dbContext.Items.AsNoTracking().ToListAsync();
    }

    public async Task<Item> CreateItemAsync(Item item)
    {
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;

        if (string.IsNullOrEmpty(item.Id))
            item.Id = ObjectId.GenerateNewId().ToString();

        _dbContext.Items.Add(item);

        await _dbContext.SaveChangesAsync();
        return item;
    }

    public async Task<Item> UpdateItemAsync(string id, Item item)
    {
        var existingItem = await _dbContext.Items.FindAsync(id);

        if (existingItem == null)
            throw new KeyNotFoundException($"Item with id {id} not found.");

        existingItem.Name = item.Name;
        existingItem.Description = item.Description;
        existingItem.Price = item.Price;
        existingItem.StockQuantity = item.StockQuantity;
        existingItem.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return existingItem;
    }

    public async Task<bool> DeleteItemAsync(string id)
    {
        var item = await _dbContext.Items.FindAsync(id);

        if (item == null)
            return false;

        _dbContext.Items.Remove(item);

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ItemExistsAsync(string id)
    {
        return await _dbContext.Items.AnyAsync(i => i.Id == id);
    }
}
