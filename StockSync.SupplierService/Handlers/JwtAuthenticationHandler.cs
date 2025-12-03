using Microsoft.IdentityModel.Tokens;
using StockSync.Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace StockSync.SupplierService.Handlers;

public class JwtAuthenticationHandler : DelegatingHandler
{
    private readonly IConfiguration _configuration;

    public JwtAuthenticationHandler(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var jwtSecretKey = _configuration["JWT_SECRET_KEY"]
            ?? throw new InvalidOperationException("JWT secret key not found in configuration.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, UserRoles.User) }),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);

        return base.SendAsync(request, cancellationToken);
    }
}
