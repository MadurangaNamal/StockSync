namespace StockSync.SupplierService.Models;

public class ItemDto
{
    public string Id { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ItemDto()
    {
        Id = string.Empty;
        Name = string.Empty;
        Price = 0m;
        StockQuantity = 0;
        UpdatedAt = default;
    }

    public ItemDto(string id, string name, decimal price, int stockQuantity, DateTime updatedAt)
    {
        Id = id;
        Name = name;
        Price = price;
        StockQuantity = stockQuantity;
        UpdatedAt = updatedAt;
    }
}
