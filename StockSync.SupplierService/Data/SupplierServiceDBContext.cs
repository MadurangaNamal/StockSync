using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Supplier>()
        .Property(s => s.Items)
        .HasConversion(                                             // Convert List<string> to a single string for storage
            v => string.Join(",", v ?? new List<string>()),
            v => v != null ? v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>()) 
        // Ensure EF Core can track changes to the List<string> property 
        .Metadata.SetValueComparer(
            new ValueComparer<List<string>>(
                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),       // Compare two lists
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            )
        );

        base.OnModelCreating(modelBuilder);
    }
}

