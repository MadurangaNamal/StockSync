namespace StockSync.SupplierService.Models;

public record SupplierDto(
    int SupplierId,
    string Name,
    string ContactEmail,
    string ContactPhone,
    string Address,
    string? City,
    string? State,
    string? ZipCode,
    string? Country);
