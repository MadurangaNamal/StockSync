using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockSync.Shared.Middlewares;
using StockSync.Shared.Models;
using StockSync.SupplierService.Data;
using StockSync.SupplierService.Infrastructure;
using StockSync.SupplierService.Services;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.ConfigureSerilog();
builder.Configuration.AddUserSecrets<Program>();

var rawConnectionString = builder.Configuration.GetConnectionString("StockSyncDBConnection")
    ?? throw new InvalidOperationException("Connection string 'StockSyncDBConnection' not found.");
var dbPassword = builder.Configuration["DB_PASSWORD"]
    ?? throw new InvalidOperationException("Database password not found in configuration.");
var jwtSecretKey = builder.Configuration["JWT_SECRET_KEY"]
    ?? throw new InvalidOperationException("JWT secret key not found in configuration.");
var connectionString = rawConnectionString.Replace("{DB_PASSWORD}", dbPassword);
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
    ClockSkew = TimeSpan.Zero,
};

//  Add Services to the container
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<ISupplierServiceRepository, SupplierServiceRepository>();
builder.Services.AddScoped<SupplierSyncService>();
builder.Services.AddDbContext<SupplierServiceDBContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddHttpClient();
builder.Services.AddSingleton(tokenValidationParameters);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(cfg => { }, typeof(Program));

builder.Services.AddHangfire(config => config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(connectionString,
            new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

builder.Services.AddHangfireServer();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = tokenValidationParameters;

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.NoResult();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    StatusCode = 401,
                    Message = "Authentication failed. Token is invalid or expired."
                }));
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    StatusCode = 401,
                    Message = "Unauthorized access. Please provide a valid token."
                }));
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    StatusCode = 403,
                    Message = "Forbidden. You do not have permission to access this resource."
                }));
            }
        };
    });

builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole(UserRoles.Admin));
        options.AddPolicy("RequireUserRole", policy => policy.RequireRole(UserRoles.User));
    });


var app = builder.Build();

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<SupplierServiceDBContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations completed successfully");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while applying database migrations");
    throw;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() ||
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseRequestResponseLogging();
app.UseHangfireDashboard("/hangfire");
app.MapControllers();

// Schedule a recurring job for suppliers-Items syncing
RecurringJob.AddOrUpdate<SupplierSyncService>(
    "sync-suppliers",
   service => service.SyncAllSuppliers(),
   Cron.Minutely); // configure the sync interval as necessary

app.Run();
