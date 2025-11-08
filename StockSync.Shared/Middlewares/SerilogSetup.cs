using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System.Text;

namespace StockSync.Shared.Middlewares;

public static class SerilogSetup
{
    /// <summary>
    /// Configures Serilog as the logging provider for the application.
    /// Reads configuration from appsettings and sets up the logger with context enrichment.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance</param>
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        // Configure Serilog logger using settings
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        // Replace the default logging provider with Serilog
        builder.Host.UseSerilog();

        // Register Serilog with the dependency injection container
        // The dispose: true parameter ensures proper cleanup when the application shuts down
        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
    }

    /// <summary>
    /// Adds comprehensive HTTP request/response logging middleware.
    /// Logs all HTTP requests with timing information and captures request/response bodies for API endpoints.
    /// </summary>
    /// <param name="app">The WebApplication instance</param>
    public static void UseRequestResponseLogging(this WebApplication app)
    {
        // Add built-in Serilog HTTP request logging with custom message template and enrichment
        app.UseSerilogRequestLogging(options =>
        {
            // Customize the log message format to include method, path, status code, and execution time
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";

            // Set all HTTP requests to log at Information level (can be customized based on status code or duration)
            options.GetLevel = (httpContext, elapsedMs, ex) => LogEventLevel.Information;

            // Add additional context information to each HTTP request log
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.ToString());
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            };
        });

        // Custom middleware to capture and log request/response bodies for API endpoints
        app.Use(async (context, next) =>
        {
            var request = context.Request;

            // Only log request/response bodies for API endpoints to avoid logging static content
            if (request.Path.StartsWithSegments("/api"))
            {
                // Enable request body buffering to allow multiple reads
                request.EnableBuffering();

                // Read and log the request body
                using var requestReader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);

                var requestBody = await requestReader.ReadToEndAsync();

                // Reset stream position so the controller can read the body
                request.Body.Position = 0;

                Log.Information("API Request: {Method} {Path} Body: {Body}",
                    request.Method, request.Path, requestBody);

                // Capture response body by intercepting the response stream
                var originalBodyStream = context.Response.Body;
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Continue to the next middleware/controller
                await next();

                // Read and log the response body
                responseBody.Seek(0, SeekOrigin.Begin);
                using var responseReader = new StreamReader(responseBody, leaveOpen: true);
                var responseBodyText = await responseReader.ReadToEndAsync();

                // Copy the response body to the original stream so it reaches the client
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;

                Log.Information("API Response: {StatusCode} Body: {Body}",
                    context.Response.StatusCode, responseBodyText);
            }
            else
            {
                // For non-API endpoints, just continue without body logging
                await next();
            }
        });
    }
}