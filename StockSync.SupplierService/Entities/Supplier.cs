#nullable enable
using System.ComponentModel.DataAnnotations;

namespace StockSync.SupplierService.Entities;

public class Supplier
{
    [Key]
    public int SupplierId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;

    [Phone]
    public string ContactPhone { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public string? Country { get; set; }
}
