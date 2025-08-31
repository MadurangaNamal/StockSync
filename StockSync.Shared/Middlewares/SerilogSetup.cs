using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System.Text;

namespace StockSync.Shared.Middlewares;

public static class SerilogSetup
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        // Configure Serilog with configuration settings
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        // Use Serilog as the logging provider for the host
        builder.Host.UseSerilog();

        // Add Serilog to the DI system, ensuring it disposes properly
        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
    }

    public static void UseRequestResponseLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
            options.GetLevel = (httpContext, elapsedMs, ex) => LogEventLevel.Information;
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.ToString());
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            };
        });

        app.Use(async (context, next) =>
        {
            var request = context.Request;
            if (request.Path.StartsWithSegments("/api"))
            {
                request.EnableBuffering();
                using var requestReader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);
                var requestBody = await requestReader.ReadToEndAsync();
                request.Body.Position = 0; // Reset for further processing
                Log.Information("Request: {Method} {Path} Body: {Body}", request.Method, request.Path, requestBody);

                var originalBodyStream = context.Response.Body;
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                await next();

                responseBody.Seek(0, SeekOrigin.Begin);
                using var responseReader = new StreamReader(responseBody, leaveOpen: true);
                var responseBodyText = await responseReader.ReadToEndAsync();
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;

                Log.Information("Response: {StatusCode} Body: {Body}", context.Response.StatusCode, responseBodyText);
            }
            else
            {
                await next();
            }
        });
    }
}
