using StockSync.Shared;

namespace StockSync.SupplierService.Models.UserIdentity;

public class RegisterUserModel : LoginModel
{
    public string Role { get; set; } = UserRoles.User;
}
