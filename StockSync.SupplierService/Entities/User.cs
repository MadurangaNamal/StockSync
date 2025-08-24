using StockSync.SupplierService.Models.UserIdentity;
using System.ComponentModel.DataAnnotations;

namespace StockSync.SupplierService.Entities;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = UserRoles.User;
}
