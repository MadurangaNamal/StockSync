using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using StockSync.ItemService.Data;
using StockSync.ItemService.Entities;

namespace StockSync.ItemService.Infrastructure;

public class ItemServiceRepository : IItemServiceRepository
{
    private readonly ItemServiceDBContext _dbContext;
    private readonly IMapper _mapper;

    public ItemServiceRepository(ItemServiceDBContext context, IMapper mapper)
    {
        _dbContext = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Item?> GetItemByIdAsync(string id)
    {
        return await _dbContext.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);
    }

    public async Task<IEnumerable<Item>> GetItemsAsync(string? itemIds)
    {
        if (!string.IsNullOrEmpty(itemIds))
        {
            var ids = itemIds.Split(',').Select(id => id.Trim()).ToList();

            return await _dbContext.Items
                .AsNoTracking()
                .Where(item => ids.Contains(item.Id))
                .ToListAsync();
        }

        return await _dbContext.Items.AsNoTracking().ToListAsync();
    }

    public async Task<Item> CreateItemAsync(Item item)
    {
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = item.CreatedAt;

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

        _mapper.Map(item, existingItem);
        existingItem.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return existingItem;
    }

    public async Task<bool> DeleteItemAsync(string id)
    {
        var deletedCount = await _dbContext.Items
            .Where(i => i.Id == id)
            .ExecuteDeleteAsync();

        return deletedCount > 0;
    }

    public async Task<bool> ItemExistsAsync(string id)
    {
        return await _dbContext.Items.AnyAsync(i => i.Id == id);
    }
}
