using System.ComponentModel.DataAnnotations;

namespace StockSync.SupplierService.Models.UserIdentity;

public class LoginModel
{
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string Username { get; set; } = default!;

    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;
}

