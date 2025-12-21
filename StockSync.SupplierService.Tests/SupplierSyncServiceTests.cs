using Microsoft.EntityFrameworkCore;
using Moq;
using StockSync.SupplierService.Data;
using StockSync.SupplierService.Entities;
using StockSync.SupplierService.Infrastructure;
using StockSync.SupplierService.Models;
using StockSync.SupplierService.Services;

namespace StockSync.SupplierService.Tests;

public class SupplierSyncServiceTests : IDisposable
{
    private bool _disposed;

    private readonly SupplierServiceDBContext _dbContext;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly SupplierSyncService _supplierSyncService;

    public SupplierSyncServiceTests()
    {
        var options = new DbContextOptionsBuilder<SupplierServiceDBContext>()
            .UseInMemoryDatabase(databaseName: "StockSyncTestDb")
            .Options;

        _dbContext = new SupplierServiceDBContext(options);
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockCacheService = new Mock<ICacheService>();

        _supplierSyncService = new SupplierSyncService(_dbContext, _mockHttpClientFactory.Object, _mockCacheService.Object);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dbContext.Database.EnsureDeleted();
                _dbContext.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SyncSupplierItems_WhenSupplierDoesNotExist_ReturnsWithoutError()
    {
        // Arrange
        int nonExistentSupplierId = 999;

        // Act
        await _supplierSyncService.SyncSupplierItems(nonExistentSupplierId);

        // Assert
        _mockHttpClientFactory.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
        _mockCacheService.Verify(x => x.SetAllItemDtos(It.IsAny<Dictionary<string, ItemDto>>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task SyncSupplierItems_WhenSupplierHasNoItems_ReturnsWithoutHttpCall()
    {
        // Arrange
        var supplier = new Supplier
        {
            SupplierId = 1,
            Name = "Test N",
            Address = "Test S",
            ContactEmail = "Test M",
            ContactPhone = "Test P",
            Items = new List<string>()
        };

        await _dbContext.Suppliers.AddAsync(supplier);
        await _dbContext.SaveChangesAsync();

        // Act
        await _supplierSyncService.SyncSupplierItems(supplier.SupplierId);

        // Assert
        _mockHttpClientFactory.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
        _mockCacheService.Verify(x => x.SetAllItemDtos(It.IsAny<Dictionary<string, ItemDto>>(), It.IsAny<TimeSpan>()), Times.Never);
    }
}
