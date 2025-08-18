using Microsoft.EntityFrameworkCore;
using StockSync.SupplierService.Entities;

namespace StockSync.SupplierService.Data;

public class SupplierServiceDBContext : DbContext
{
    public SupplierServiceDBContext(DbContextOptions<SupplierServiceDBContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
}

