using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using StockSync.SupplierService.Data;
using StockSync.SupplierService.Entities;
using StockSync.SupplierService.Infrastructure;
using StockSync.SupplierService.Models;
using StockSync.SupplierService.Services;
using System.Net;
using System.Text.Json;

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

    [Fact]
    public async Task SyncSupplierItems_WhenSupplierHasNullItems_ReturnsWithoutHttpCall()
    {
        // Arrange
        var supplier = new Supplier
        {
            SupplierId = 1,
            Name = "Test N",
            Address = "Test S",
            ContactEmail = "Test M",
            ContactPhone = "Test P",
            Items = null
        };

        await _dbContext.Suppliers.AddAsync(supplier);
        await _dbContext.SaveChangesAsync();

        // Act
        await _supplierSyncService.SyncSupplierItems(supplier.SupplierId);

        // Assert
        _mockHttpClientFactory.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SyncSupplierItems_SuccessfulSync_UpdatesSupplierAndCache()
    {
        // Arrange
        var supplier = new Supplier
        {
            SupplierId = 1,
            Name = "Test N",
            Address = "Test S",
            ContactEmail = "Test M",
            ContactPhone = "Test P",
            Items = new List<string> { "1", "2", "3" }
        };

        await _dbContext.Suppliers.AddAsync(supplier);
        await _dbContext.SaveChangesAsync();

        var itemDtos = new List<ItemDto>
        {
            new ItemDto { Id = "1", Name = "Item 1" },
            new ItemDto { Id = "2", Name = "Item 2" },
            new ItemDto { Id = "3", Name = "Item 3" }
        };

        var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, itemDtos);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://testuri.com")
        };

        _mockHttpClientFactory.Setup(x => x.CreateClient("ItemServiceClient")).Returns(httpClient);

        // Act
        await _supplierSyncService.SyncSupplierItems(supplier.SupplierId);

        // Assert
        var updatedSupplier = await _dbContext.Suppliers.FindAsync(supplier.SupplierId);

        Assert.NotNull(updatedSupplier);
        Assert.NotNull(updatedSupplier.Items);
        Assert.Equal(3, updatedSupplier.Items.Count);
        Assert.Contains("1", updatedSupplier.Items);
        Assert.Contains("2", updatedSupplier.Items);
        Assert.Contains("3", updatedSupplier.Items);

        _mockCacheService.Verify(x => x.SetAllItemDtos(
            It.Is<Dictionary<string, ItemDto>>(d => d.Count == 3),
            It.Is<TimeSpan>(t => t == TimeSpan.FromHours(1))),
            Times.Once);
    }

    [Fact]
    public async Task SyncSupplierItems_WhenApiReturnsSubsetOfItems_UpdatesSupplierWithReturnedItems()
    {
        // Arrange
        var supplier = new Supplier
        {
            SupplierId = 1,
            Name = "Test N",
            Address = "Test S",
            ContactEmail = "Test M",
            ContactPhone = "Test P",
            Items = new List<string> { "1", "2", "3", "4", "5" }
        };

        await _dbContext.Suppliers.AddAsync(supplier);
        await _dbContext.SaveChangesAsync();

        var itemDtos = new List<ItemDto>
        {
            new ItemDto { Id = "1", Name = "Item 1" },
            new ItemDto { Id = "2", Name = "Item 2" },
            new ItemDto { Id = "3", Name = "Item 3" }
        };

        var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, itemDtos);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://testuri.com")
        };

        _mockHttpClientFactory.Setup(x => x.CreateClient("ItemServiceClient")).Returns(httpClient);

        // Act
        await _supplierSyncService.SyncSupplierItems(supplier.SupplierId);

        // Assert
        var updatedSupplier = await _dbContext.Suppliers.FindAsync(supplier.SupplierId);

        Assert.NotNull(updatedSupplier);
        Assert.NotNull(updatedSupplier.Items);
        Assert.Equal(3, updatedSupplier.Items.Count);
        Assert.Contains("1", updatedSupplier.Items);
        Assert.Contains("2", updatedSupplier.Items);
        Assert.Contains("3", updatedSupplier.Items);
        Assert.DoesNotContain("4", updatedSupplier.Items);
        Assert.DoesNotContain("5", updatedSupplier.Items);

        _mockCacheService.Verify(x => x.SetAllItemDtos(
            It.Is<Dictionary<string, ItemDto>>(d => d.Count == 3),
            It.Is<TimeSpan>(t => t == TimeSpan.FromHours(1))),
            Times.Once);
    }

    [Fact]
    public async Task SyncSupplierItems_FirstAttemptFails_RetriesAndSucceeds()
    {
        // Arrange
        var supplier = new Supplier
        {
            SupplierId = 1,
            Name = "Test N",
            Address = "Test S",
            ContactEmail = "Test M",
            ContactPhone = "Test P",
            Items = new List<string> { "1", "2" }
        };

        await _dbContext.Suppliers.AddAsync(supplier);
        await _dbContext.SaveChangesAsync();

        var itemDtos = new List<ItemDto>
        {
            new ItemDto { Id = "1", Name = "Item 1" },
            new ItemDto { Id = "2", Name = "Item 2" },
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        int retryCount = 0;

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                retryCount++;

                if (retryCount == 1)
                    throw new HttpRequestException("Network error!");

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(itemDtos))
                };
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://testuri.com")
        };

        _mockHttpClientFactory.Setup(x => x.CreateClient("ItemServiceClient")).Returns(httpClient);

        // Act
        await _supplierSyncService.SyncSupplierItems(supplier.SupplierId);

        // Assert
        Assert.Equal(2, retryCount);

        var updatedSupplier = await _dbContext.Suppliers.FindAsync(supplier.SupplierId);
        Assert.NotNull(updatedSupplier);
        Assert.NotNull(updatedSupplier.Items);
        Assert.Equal(2, updatedSupplier.Items.Count);

        _mockCacheService.Verify(x => x.SetAllItemDtos(
            It.Is<Dictionary<string, ItemDto>>(d => d.Count == 2),
            It.Is<TimeSpan>(t => t == TimeSpan.FromHours(1))),
            Times.Once);
    }

    [Fact]
    public async Task SyncSupplierItems_AllAttemptsFailWithHttpException_ThrowsException()
    {
        // Arrange
        var supplier = new Supplier
        {
            SupplierId = 1,
            Name = "Test N",
            Address = "Test S",
            ContactEmail = "Test M",
            ContactPhone = "Test P",
            Items = new List<string> { "1" }
        };

        await _dbContext.Suppliers.AddAsync(supplier);
        await _dbContext.SaveChangesAsync();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network Error!"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://testuri.com")
        };

        _mockHttpClientFactory.Setup(x => x.CreateClient("ItemServiceClient")).Returns(httpClient);

        // Act & Aeert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _supplierSyncService.SyncSupplierItems(supplier.SupplierId));

        _mockCacheService.Verify(x => x.SetAllItemDtos(It.IsAny<Dictionary<string, ItemDto>>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task SyncSupplierItems_ApiReturnsNonSuccessStatusCode_DoesNotUpdateSupplier()
    {
        // Arrange
        var supplier = new Supplier
        {
            SupplierId = 1,
            Name = "Test N",
            Address = "Test S",
            ContactEmail = "Test M",
            ContactPhone = "Test P",
            Items = new List<string> { "1" }
        };

        await _dbContext.Suppliers.AddAsync(supplier);
        await _dbContext.SaveChangesAsync();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://testuri.com")
        };

        _mockHttpClientFactory.Setup(x => x.CreateClient("ItemServiceClient")).Returns(httpClient);

        // Act
        await _supplierSyncService.SyncSupplierItems(supplier.SupplierId);

        // Assert
        var updatedSupplier = await _dbContext.Suppliers.FindAsync(supplier.SupplierId);
        Assert.NotNull(updatedSupplier);
        Assert.NotNull(updatedSupplier.Items);
        Assert.Single(updatedSupplier.Items); // Supplier items remains unchanged

        _mockCacheService.Verify(x => x.SetAllItemDtos(It.IsAny<Dictionary<string, ItemDto>>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task SyncSupplierItems_ApiReturnsNull_DoesNotUpdateSupplier()
    {
        // Arrange
        var supplier = new Supplier
        {
            SupplierId = 1,
            Name = "Test N",
            Address = "Test S",
            ContactEmail = "Test M",
            ContactPhone = "Test P",
            Items = new List<string> { "1", "2" }
        };

        await _dbContext.Suppliers.AddAsync(supplier);
        await _dbContext.SaveChangesAsync();

        var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, null!);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://testuri.com")
        };

        _mockHttpClientFactory.Setup(x => x.CreateClient("ItemServiceClient")).Returns(httpClient);

        // Act
        await _supplierSyncService.SyncSupplierItems(supplier.SupplierId);

        // Assert
        _mockCacheService.Verify(x => x.SetAllItemDtos(It.IsAny<Dictionary<string, ItemDto>>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task SyncSupplierItems_CorrectQueryStringFormatted()
    {
        // Arrange
        var supplier = new Supplier
        {
            SupplierId = 1,
            Name = "Test N",
            Address = "Test S",
            ContactEmail = "Test M",
            ContactPhone = "Test P",
            Items = new List<string> { "111", "215" }
        };

        await _dbContext.Suppliers.AddAsync(supplier);
        await _dbContext.SaveChangesAsync();

        var itemDtos = new List<ItemDto>
        {
            new ItemDto { Id = "111", Name = "Item ABC" },
            new ItemDto { Id = "215", Name = "Item XYZ" },
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        HttpRequestMessage capturedRequest = null!;

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                capturedRequest = request;
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(itemDtos))
                };
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://testuri.com")
        };

        _mockHttpClientFactory.Setup(x => x.CreateClient("ItemServiceClient")).Returns(httpClient);

        // Act
        await _supplierSyncService.SyncSupplierItems(supplier.SupplierId);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.RequestUri);
        Assert.Contains("itemIds=111,215", capturedRequest.RequestUri.ToString());
    }

    // Helpers
    private Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpStatusCode statusCode, List<ItemDto> responseData)
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = responseData != null
                    ? new StringContent(JsonSerializer.Serialize(responseData))
                    : new StringContent("null")
            });

        return mockHttpMessageHandler;
    }
}
