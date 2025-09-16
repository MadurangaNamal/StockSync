using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace StockSync.Shared.Middlewares;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request.");

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var errorResponse = new ProblemDetails
            {
                Status = 500,
                Detail = "An unexpected error occurred. Please try again later."
            };

            if (!context.Response.HasStarted)
            {
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
            else
            {
                // Log the error but avoid writing to a closed stream
                Serilog.Log.Error(ex, "Unhandled exception occurred.");
            }
        }
    }
}

// An extension method to easily add the middleware to the application pipeline
public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandler>();
    }
}
