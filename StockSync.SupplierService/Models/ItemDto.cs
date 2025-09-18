namespace StockSync.SupplierService.Models;

public record ItemDto(string Id, string Name, decimal Price, int StockQuantity, DateTime UpdatedAt);
