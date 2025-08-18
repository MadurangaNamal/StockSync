using Microsoft.EntityFrameworkCore;
using StockSync.SupplierService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration.AddUserSecrets<Program>();

var rawConnectionString = builder.Configuration.GetConnectionString("StockSyncDBConnection")
    ?? throw new InvalidOperationException("Connection string 'StockSyncDBConnection' not found.");

var dbPassword = builder.Configuration["DB_PASSWORD"]
    ?? throw new InvalidOperationException("Database password 'DB_PASSWORD' not found in configuration.");

var connectionString = rawConnectionString.Replace("{DB_PASSWORD}", dbPassword);

builder.Services.AddDbContext<SupplierServiceDBContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();

}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
