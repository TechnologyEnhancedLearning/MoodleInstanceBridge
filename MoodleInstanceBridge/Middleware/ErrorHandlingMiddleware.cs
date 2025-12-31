using System.Text.Json;

namespace MoodleInstanceBridge.Middleware
{
    /// <summary>
    /// Middleware to handle and log errors with structured context
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly string _instanceId;
        private readonly IHostEnvironment _environment;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
            _instanceId = configuration["InstanceId"] 
                ?? Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") 
                ?? Environment.MachineName;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var correlationId = context.TraceIdentifier;

            // Structured error logging
            _logger.LogError(
                exception,
                "Unhandled exception occurred | Method: {Method} | Path: {Path} | CorrelationId: {CorrelationId} | InstanceId: {InstanceId} | ExceptionType: {ExceptionType}",
                context.Request.Method,
                context.Request.Path,
                correlationId,
                _instanceId,
                exception.GetType().Name
            );

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = new
            {
                error = "An error occurred processing your request.",
                correlationId = correlationId,
                instanceId = _instanceId,
                // Only include stack trace in development
                stackTrace = _environment.IsDevelopment() ? exception.ToString() : null
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Extension method to register the ErrorHandlingMiddleware
    /// </summary>
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
