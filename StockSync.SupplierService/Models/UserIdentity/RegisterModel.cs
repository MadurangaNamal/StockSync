namespace StockSync.SupplierService.Models.UserIdentity;

public class RegisterModel : LoginModel
{
    public string Role { get; set; } = UserRoles.User;
}
