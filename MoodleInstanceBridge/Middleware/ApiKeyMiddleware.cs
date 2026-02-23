using Microsoft.Extensions.Primitives;
using MoodleInstanceBridge.Telemetry;
using System.Net;

namespace MoodleInstanceBridge.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeader = "X-API-KEY";
        private readonly ISecurityTelemetryService _securityTelemetry;
        public ApiKeyMiddleware(RequestDelegate next, ISecurityTelemetryService securityTelemetry)
        {
            _next = next;
            _securityTelemetry = securityTelemetry;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            // Allow swagger & health
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/health"))
            {
                await _next(context);
                return;
            }
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                await _next(context);
                return;
            }
            if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out StringValues providedKey))
            {
                _securityTelemetry.TrackAuthFailure(
                                                  "API_KEY_MISSING",
                                                  context.Request.Path,
                                                  context.Connection.RemoteIpAddress?.ToString(),
                                                  "API key header not provided");

                                                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                                            await context.Response.WriteAsJsonAsync(new
                                                            {
                                                                error = new { code = "API_KEY_MISSING", message = "API key is missing" }
                                                            });
                return;
            }

            var configuredKeys = configuration
                .GetSection("ApiKeys")
                .Get<Dictionary<string, string>>();

            if (configuredKeys == null ||
                !configuredKeys.Any(k => k.Value == providedKey))
            {
                await WriteError(context, "API_KEY_INVALID", "Invalid API key");
                return;
            }

            // OPTIONAL: attach client name
            var clientName = configuredKeys.First(k => k.Value == providedKey).Key;
            context.Items["ClientName"] = clientName;

            await _next(context);
        }

        private static async Task WriteError(HttpContext context, string code, string message)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code,
                    message
                }
            });
        }
    }
}
