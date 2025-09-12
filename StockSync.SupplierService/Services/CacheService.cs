using Microsoft.Extensions.Caching.Memory;
using StockSync.SupplierService.Infrastructure;
using StockSync.SupplierService.Models;

namespace StockSync.SupplierService.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private const string CacheKeyPrefix = "Item_";

    public CacheService(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public void SetItemDto(string itemId, ItemDto itemDto, TimeSpan? expiry = null)
    {
        _cache.Set($"{CacheKeyPrefix}{itemId}", itemDto, expiry ?? TimeSpan.FromHours(1));
    }

    public ItemDto? GetItemDto(string itemId)
    {
        return _cache.TryGetValue($"{CacheKeyPrefix}{itemId}", out ItemDto? itemDto) ? itemDto : null;
    }

    public void SetAllItemDtos(Dictionary<string, ItemDto> items, TimeSpan? expiry = null)
    {
        foreach (var kvp in items)
        {
            SetItemDto(kvp.Key, kvp.Value, expiry);
        }
    }
}
