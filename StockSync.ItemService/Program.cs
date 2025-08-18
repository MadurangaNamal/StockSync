using Microsoft.EntityFrameworkCore;
using StockSync.ItemService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var mongoConn = builder.Configuration["MongoDB:ConnectionURI"]
    ?? throw new InvalidOperationException("MongoDB connection string is not configured.");
var dbName = builder.Configuration["MongoDB:DatabaseName"]
    ?? throw new InvalidOperationException("MongoDB database name is not configured.");

builder.Services.AddDbContext<ItemServiceDBContext>(options => options.UseMongoDB(mongoConn, dbName));
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
