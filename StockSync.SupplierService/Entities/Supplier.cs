#nullable enable
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace StockSync.SupplierService.Entities;

public class Supplier
{
    [Key]
    public int SupplierId { get; set; }

    [Required]
    public string Name { get; set; } = default!;

    [Required]
    [EmailAddress]
    public string ContactEmail { get; set; } = default!;

    [Required]
    [Phone]
    public string ContactPhone { get; set; } = default!;

    [Required]
    [MaxLength(1000)]
    public string Address { get; set; } = default!;

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public string? Country { get; set; }

    [BsonIgnore]
    public List<string>? Items { get; set; }
}
