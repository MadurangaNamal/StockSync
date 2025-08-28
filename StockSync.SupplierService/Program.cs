using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockSync.Shared;
using StockSync.SupplierService.Data;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

builder.Services.AddDbContext<SupplierServiceDBContext>(
    options => options.UseSqlServer(connectionString));

builder.Services.AddSingleton(tokenValidationParameters);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(cfg => { }, typeof(Program));

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
