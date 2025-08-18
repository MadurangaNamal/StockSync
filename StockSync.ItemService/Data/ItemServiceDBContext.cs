using Microsoft.EntityFrameworkCore;
using StockSync.ItemService.Entities;

namespace StockSync.ItemService.Data;

public class ItemServiceDBContext : DbContext
{
    public ItemServiceDBContext(DbContextOptions<ItemServiceDBContext> options)
        : base(options)
    {
    }

    public DbSet<Item> Items { get; set; }
}

