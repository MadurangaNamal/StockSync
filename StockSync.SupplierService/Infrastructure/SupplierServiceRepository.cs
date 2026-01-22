using Microsoft.EntityFrameworkCore;
using StockSync.Shared.Models;
using StockSync.SupplierService.Data;
using StockSync.SupplierService.Entities;

namespace StockSync.SupplierService.Infrastructure;

public class SupplierServiceRepository : ISupplierServiceRepository
{
    private readonly SupplierServiceDBContext _dbContext;

    public SupplierServiceRepository(SupplierServiceDBContext dBContext)
    {
        _dbContext = dBContext ?? throw new ArgumentNullException(nameof(dBContext));
    }

    public async Task<PagedResult<Supplier>> GetSuppliersAsync(PaginationParams pagination)
    {
        var query = _dbContext.Suppliers.AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return PagedResult<Supplier>.Create(items, pagination.PageNumber, pagination.PageSize, totalCount);
    }

    public async Task<Supplier?> GetSupplierAsync(int supplierId)
    {
        return await _dbContext.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId);
    }

    public async Task AddSupplierAsync(Supplier supplier)
    {
        ArgumentNullException.ThrowIfNull(supplier);

        await _dbContext.Suppliers.AddAsync(supplier);
        await SaveAsync();
    }

    public async Task UpdateSupplier(Supplier supplier)
    {
        ArgumentNullException.ThrowIfNull(supplier);

        _dbContext.Suppliers.Update(supplier);
        await SaveAsync();
    }

    public async Task DeleteSupplier(Supplier supplier)
    {
        ArgumentNullException.ThrowIfNull(supplier);

        _dbContext.Suppliers.Remove(supplier);
        await SaveAsync();
    }

    public async Task<bool> SupplierExistsAsync(int supplierId)
    {
        return await _dbContext.Suppliers
            .AsNoTracking()
            .AnyAsync(s => s.SupplierId == supplierId);
    }

    public async Task<bool> SaveAsync()
    {
        return await _dbContext.SaveChangesAsync() >= 0;
    }
}
