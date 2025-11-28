using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace StockSync.ItemService.Entities;

public class Item
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = default!;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
