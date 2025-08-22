using Microsoft.EntityFrameworkCore;
using StockSync.ItemService.Data;
using StockSync.ItemService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var mongoConn = builder.Configuration["MongoDB:ConnectionURI"]
    ?? throw new InvalidOperationException("MongoDB connection string is not configured.");
var dbName = builder.Configuration["MongoDB:DatabaseName"]
    ?? throw new InvalidOperationException("MongoDB database name is not configured.");

builder.Services.AddDbContext<ItemServiceDBContext>(options => options.UseMongoDB(mongoConn, dbName));
builder.Services.AddScoped<IItemServiceRepository, ItemServiceRepository>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(cfg => { }, typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
