using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockSync.Shared.Models;
using StockSync.SupplierService.Data;
using StockSync.SupplierService.Infrastructure;
using StockSync.SupplierService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace StockSync.SupplierService.Services;

public class SupplierSyncService
{
    private readonly SupplierServiceDBContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ICacheService _cacheService;

    public SupplierSyncService(SupplierServiceDBContext dbContext,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ICacheService cacheService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task SyncSupplierItems(int supplierId)
    {
        var supplier = await _dbContext.Suppliers.FindAsync(supplierId);
        if (supplier == null)
            return;

        var currentItemIds = supplier.Items ?? [];

        if (!currentItemIds.Any())
            return;

        var itemIdsParam = string.Join(",", currentItemIds);
        var client = CreateAuthorizedClient();

        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var response = await client.GetAsync($"api/items?itemIds={itemIdsParam}");

                if (response.IsSuccessStatusCode)
                {
                    var items = await response.Content.ReadFromJsonAsync<List<ItemDto>>();

                    if (items != null)
                    {
                        // Update supplier.Items with valid IDs
                        supplier.Items = items.Select(i => i.Id).ToList();
                        var itemDict = items.ToDictionary(i => i.Id, i => i);

                        _cacheService.SetAllItemDtos(itemDict, TimeSpan.FromHours(1));
                    }

                    await _dbContext.SaveChangesAsync();
                    return;
                }
            }
            catch (HttpRequestException)
            {
                if (attempt == 2) throw;
                await Task.Delay(1000 * (attempt + 1));
            }
        }
    }

    public async Task SyncAllSuppliers()
    {
        var suppliers = await _dbContext.Suppliers.ToListAsync();
        foreach (var supplier in suppliers)
        {
            await SyncSupplierItems(supplier.SupplierId);
        }
    }

    private HttpClient CreateAuthorizedClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_configuration["ItemService:BaseUrl"] ??
            throw new InvalidOperationException("BaseUrl not found in configuration."));

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

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
        return client;
    }

}
