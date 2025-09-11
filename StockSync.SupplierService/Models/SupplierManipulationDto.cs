namespace StockSync.SupplierService.Models;

public record SupplierManipulationDto(
    string Name,
    string ContactEmail,
    string ContactPhone,
    string Address,
    string? City,
    string? State,
    string? ZipCode,
    string? Country,
    List<string>? Items = null);
