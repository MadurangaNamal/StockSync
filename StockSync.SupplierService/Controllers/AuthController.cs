using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockSync.SupplierService.Data;
using StockSync.SupplierService.Entities;
using StockSync.SupplierService.Models.UserIdentity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StockSync.SupplierService.Controllers;

[ApiController]
[Route("api/[controller]/user")]
public class AuthController : ControllerBase
{
    private readonly SupplierServiceDBContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthController(SupplierServiceDBContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    [HttpGet("{userId}", Name = "GetUser")]
    public async Task<ActionResult<User>> GetUserById(string userId)
    {
        var user = await _dbContext.Users.FindAsync(int.Parse(userId));
        if (user == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = NotFound().StatusCode,
                Title = "User not found",
                Detail = $"No user found with ID '{userId}'."
            });
        }
        return Ok(new { user.UserId, user.Username, user.Role });
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterNewUser([FromBody] RegisterUserModel registerUserModel)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Username == registerUserModel.Username))
        {
            return BadRequest(new ProblemDetails
            {
                Status = BadRequest().StatusCode,
                Title = "User already exists",
                Detail = $"A user with username '{registerUserModel.Username}' already exists."
            });
        }

        var user = new User
        {
            Username = registerUserModel.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerUserModel.Password),
            Role = registerUserModel.Role
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return CreatedAtRoute("GetUser", new { userId = user.UserId }, new { user.UserId, user.Username, user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        var user = await _dbContext.Users.SingleAsync(u => u.Username == loginModel.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginModel.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var token = GenerateJwtToken(user);
        return Ok(new { Token = token });
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
        };

        var jwtSecretKey = _configuration["JWT_SECRET_KEY"]
            ?? throw new InvalidOperationException("JWT secret key not found in configuration.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
