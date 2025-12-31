using System.Diagnostics;

namespace MoodleInstanceBridge.Middleware
{
    /// <summary>
    /// Middleware to log request timing and metadata
    /// </summary>
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimingMiddleware> _logger;
        private readonly string _instanceId;

        public RequestTimingMiddleware(
            RequestDelegate next, 
            ILogger<RequestTimingMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _instanceId = configuration["InstanceId"] 
                ?? Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") 
                ?? Environment.MachineName;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = context.TraceIdentifier;

            // Log request start
            _logger.LogInformation(
                "Request started: {Method} {Path} | CorrelationId: {CorrelationId} | InstanceId: {InstanceId}",
                context.Request.Method,
                context.Request.Path,
                correlationId,
                _instanceId
            );

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                // Log request completion
                _logger.LogInformation(
                    "Request completed: {Method} {Path} | StatusCode: {StatusCode} | Duration: {DurationMs}ms | CorrelationId: {CorrelationId} | InstanceId: {InstanceId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    correlationId,
                    _instanceId
                );
            }
        }
    }

    /// <summary>
    /// Extension method to register the RequestTimingMiddleware
    /// </summary>
    public static class RequestTimingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestTimingMiddleware>();
        }
    }
}
