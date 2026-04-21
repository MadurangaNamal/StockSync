namespace StockSync.SupplierService.Models;

public class PagedItemResponse
{
    public List<ItemDto> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int TotalCount { get; set; }
}
