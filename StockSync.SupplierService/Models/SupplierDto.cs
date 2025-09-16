namespace StockSync.SupplierService.Models;

public class SupplierDto
{
    public int SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public List<ItemDto>? Items { get; set; } = null;

    public SupplierDto(int supplierId,
        string name,
        string contactEmail,
        string contactPhone,
        string address,
        string? city,
        string? state,
        string? zipCode,
        string? country,
        List<ItemDto>? items)
    {
        SupplierId = supplierId;
        Name = name;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        Address = address;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        Items = items;
    }
}


