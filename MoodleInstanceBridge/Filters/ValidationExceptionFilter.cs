using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MoodleInstanceBridge.Models.Errors;
using System.Text.Json;

namespace MoodleInstanceBridge.Filters
{
    /// <summary>
    /// Filter to automatically convert model validation errors to standardized error responses
    /// </summary>
    public class ValidationExceptionFilter : IActionFilter
    {
        private readonly IHostEnvironment _environment;
        private readonly string _instanceId;

        public ValidationExceptionFilter(IConfiguration configuration, IHostEnvironment environment)
        {
            _environment = environment;
            _instanceId = configuration["InstanceId"]
                ?? Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")
                ?? Environment.MachineName;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = new Dictionary<string, string[]>();

                foreach (var entry in context.ModelState)
                {
                    if (entry.Value.Errors.Any())
                    {
                        errors[entry.Key] = entry.Value.Errors
                            .Select(e => e.ErrorMessage)
                            .ToArray();
                    }
                }

                var errorResponse = new ValidationErrorResponse
                {
                    CorrelationId = context.HttpContext.TraceIdentifier,
                    InstanceId = _instanceId,
                    Errors = errors
                };

                context.Result = new BadRequestObjectResult(errorResponse);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed after execution
        }
    }
}
