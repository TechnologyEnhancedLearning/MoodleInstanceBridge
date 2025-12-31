using System.Text.Json;
using MoodleInstanceBridge.Models.Errors;

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
                "Exception occurred | Method: {Method} | Path: {Path} | CorrelationId: {CorrelationId} | InstanceId: {InstanceId} | ExceptionType: {ExceptionType}",
                context.Request.Method,
                context.Request.Path,
                correlationId,
                _instanceId,
                exception.GetType().Name
            );

            context.Response.ContentType = "application/json";

            ErrorResponse errorResponse;

            // Handle different exception types
            switch (exception)
            {
                case ValidationException validationEx:
                    context.Response.StatusCode = validationEx.StatusCode;
                    errorResponse = new ValidationErrorResponse
                    {
                        CorrelationId = correlationId,
                        InstanceId = _instanceId,
                        Errors = validationEx.ValidationErrors,
                        Detail = _environment.IsDevelopment() ? exception.ToString() : null
                    };
                    break;

                case MoodleUpstreamException moodleEx:
                    context.Response.StatusCode = moodleEx.StatusCode;
                    errorResponse = new ErrorResponse
                    {
                        ErrorCode = nameof(ErrorCode.MoodleUpstreamError),
                        Message = moodleEx.Message,
                        CorrelationId = correlationId,
                        InstanceId = _instanceId,
                        Detail = _environment.IsDevelopment() ? exception.ToString() : null,
                        Metadata = new Dictionary<string, object>
                        {
                            ["moodleInstanceId"] = moodleEx.MoodleInstanceId ?? "unknown",
                            ["moodleStatusCode"] = moodleEx.MoodleStatusCode ?? 0
                        }
                    };
                    break;

                case StandardException standardEx:
                    context.Response.StatusCode = standardEx.StatusCode;
                    errorResponse = new ErrorResponse
                    {
                        ErrorCode = standardEx.ErrorCode.ToString(),
                        Message = standardEx.Message,
                        CorrelationId = correlationId,
                        InstanceId = _instanceId,
                        Detail = _environment.IsDevelopment() ? exception.ToString() : null,
                        Metadata = standardEx.Metadata
                    };
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    errorResponse = new ErrorResponse
                    {
                        ErrorCode = nameof(ErrorCode.InternalServerError),
                        Message = "An unexpected error occurred while processing your request.",
                        CorrelationId = correlationId,
                        InstanceId = _instanceId,
                        Detail = _environment.IsDevelopment() ? exception.ToString() : null
                    };
                    break;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse, options);
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
