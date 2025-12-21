using Microsoft.EntityFrameworkCore;
using StockSync.SupplierService.Data;
using StockSync.SupplierService.Infrastructure;
using StockSync.SupplierService.Models;

namespace StockSync.SupplierService.Services;

public class SupplierSyncService
{
    private readonly SupplierServiceDBContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICacheService _cacheService;

    public SupplierSyncService(SupplierServiceDBContext dbContext, IHttpClientFactory httpClientFactory, ICacheService cacheService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    // Sync item information for a supplier with item service
    public async Task SyncSupplierItems(int supplierId)
    {
        var supplier = await _dbContext.Suppliers.FindAsync(supplierId);

        if (supplier == null)
            return;

        var currentItemIds = supplier.Items ?? [];

        if (currentItemIds.Count == 0)
            return;

        var itemIdsParam = string.Join(",", currentItemIds);
        var httpClient = CreateAuthorizedHttpClient();

        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var response = await httpClient.GetAsync($"api/items?itemIds={itemIdsParam}");

                if (response.IsSuccessStatusCode)
                {
                    var items = await response.Content.ReadFromJsonAsync<List<ItemDto>>();

                    if (items != null)
                    {
                        // Update supplier.Items with valid IDs
                        supplier.Items = items.Select(i => i.Id).ToList();
                        var itemDict = items.ToDictionary(i => i.Id, i => i);

                        _cacheService.SetAllItemDtos(itemDict, TimeSpan.FromHours(1));
                    }

                    await _dbContext.SaveChangesAsync();
                    return;
                }
            }
            catch (HttpRequestException)
            {
                if (attempt == 2)
                    throw;

                await Task.Delay(1000 * (attempt + 1));
            }
        }
    }

    public async Task SyncAllSuppliers()
    {
        var suppliers = await _dbContext.Suppliers.ToListAsync();

        foreach (var supplier in suppliers)
        {
            await SyncSupplierItems(supplier.SupplierId);
        }
    }

    private HttpClient CreateAuthorizedHttpClient()
    {
        return _httpClientFactory.CreateClient("ItemServiceClient");
    }
}
