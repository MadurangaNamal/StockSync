using StockSync.Shared.Models;
using StockSync.SupplierService.Entities;

namespace StockSync.SupplierService.Infrastructure;

public interface ISupplierServiceRepository
{
    Task<PagedResult<Supplier>> GetSuppliersAsync(PaginationParams pagination);
    Task<Supplier?> GetSupplierAsync(int supplierId);
    Task AddSupplierAsync(Supplier supplier);
    Task UpdateSupplier(Supplier supplier);
    Task DeleteSupplier(Supplier supplier);
    Task<bool> SupplierExistsAsync(int supplierId);
    Task<bool> SaveAsync();
}
