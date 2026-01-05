using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using MoodleInstanceBridge.Models.Errors;


public class SwaggerConfig : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public SwaggerConfig(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    { 
        // API Key definition
        options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Description = "Enter API Key in header: X-API-KEY",
            Name = "X-API-KEY",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    }
                },
                Array.Empty<string>()
            }
        });
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Moodle Instance Bridge API",
                Version = description.ApiVersion.ToString(),
                Description = @"
## Error Handling

All endpoints return standardized error responses with the following structure:

### Standard Error Response
```json
{
  ""errorCode"": ""InternalServerError"",
  ""message"": ""An unexpected error occurred"",
  ""correlationId"": ""trace-id"",
  ""instanceId"": ""instance-001"",
  ""timestamp"": ""2025-12-16T16:55:00Z"",
  ""detail"": ""Technical details (only in development)"",
  ""metadata"": {}
}
```

### Error Codes
- **ValidationError** (400): Request validation failed
- **AuthenticationError** (401): Authentication required or failed
- **AuthorizationError** (403): Insufficient permissions
- **NotFoundError** (404): Resource not found
- **InternalServerError** (500): Unexpected server error
- **MoodleUpstreamError** (502): Upstream Moodle instance error
- **PartialInstanceFailure** (207): Some instances failed in aggregation
- **AllInstancesFailure** (503): All instances failed
- **TimeoutError** (504): Request timeout
- **ServiceUnavailable** (503): Service temporarily unavailable
- **BadRequest** (400): Invalid or malformed request

### Validation Error Response
For validation errors, additional field-specific details are provided:
```json
{
  ""errorCode"": ""ValidationError"",
  ""message"": ""One or more validation errors occurred."",
  ""errors"": {
    ""fieldName"": [""Error message 1"", ""Error message 2""]
  },
  ""correlationId"": ""trace-id"",
  ""instanceId"": ""instance-001"",
  ""timestamp"": ""2025-12-16T16:55:00Z""
}
```

### Partial Failure Response
For multi-instance aggregation with partial failures:
```json
{
  ""errorCode"": ""PartialInstanceFailure"",
  ""message"": ""Some instances failed to respond."",
  ""successCount"": 2,
  ""failureCount"": 1,
  ""totalCount"": 3,
  ""failedInstances"": [
    {
      ""instanceId"": ""instance-003"",
      ""error"": ""Connection timeout"",
      ""statusCode"": 504
    }
  ],
  ""correlationId"": ""trace-id"",
  ""instanceId"": ""bridge-001"",
  ""timestamp"": ""2025-12-16T16:55:00Z""
}
```
"
            });
        }

        // Include XML comments if available
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "MoodleInstanceBridge.xml"), true);

        // Add common error response schemas
        options.MapType<ErrorResponse>(() => new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["errorCode"] = new OpenApiSchema { Type = "string", Description = "Stable error code for programmatic handling" },
                ["message"] = new OpenApiSchema { Type = "string", Description = "Human-readable error message" },
                ["detail"] = new OpenApiSchema { Type = "string", Description = "Technical details (development only)", Nullable = true },
                ["correlationId"] = new OpenApiSchema { Type = "string", Description = "Correlation ID for tracing" },
                ["instanceId"] = new OpenApiSchema { Type = "string", Description = "Instance that processed the request" },
                ["timestamp"] = new OpenApiSchema { Type = "string", Format = "date-time", Description = "When the error occurred" },
                ["metadata"] = new OpenApiSchema { Type = "object", Nullable = true, Description = "Additional error context" }
            },
            Required = new HashSet<string> { "errorCode", "message", "correlationId", "instanceId", "timestamp" }
        });

        options.MapType<ValidationErrorResponse>(() => new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["errorCode"] = new OpenApiSchema { Type = "string", Description = "Always 'ValidationError'" },
                ["message"] = new OpenApiSchema { Type = "string", Description = "Human-readable error message" },
                ["errors"] = new OpenApiSchema
                {
                    Type = "object",
                    AdditionalProperties = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema { Type = "string" }
                    },
                    Description = "Field-specific validation errors"
                },
                ["correlationId"] = new OpenApiSchema { Type = "string", Description = "Correlation ID for tracing" },
                ["instanceId"] = new OpenApiSchema { Type = "string", Description = "Instance that processed the request" },
                ["timestamp"] = new OpenApiSchema { Type = "string", Format = "date-time", Description = "When the error occurred" }
            },
            Required = new HashSet<string> { "errorCode", "message", "errors", "correlationId", "instanceId", "timestamp" }
        });

        options.MapType<PartialFailureResponse>(() => new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["errorCode"] = new OpenApiSchema { Type = "string", Description = "Always 'PartialInstanceFailure'" },
                ["message"] = new OpenApiSchema { Type = "string", Description = "Human-readable error message" },
                ["successCount"] = new OpenApiSchema { Type = "integer", Description = "Number of successful instances" },
                ["failureCount"] = new OpenApiSchema { Type = "integer", Description = "Number of failed instances" },
                ["totalCount"] = new OpenApiSchema { Type = "integer", Description = "Total number of instances" },
                ["failedInstances"] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["instanceId"] = new OpenApiSchema { Type = "string" },
                            ["error"] = new OpenApiSchema { Type = "string" },
                            ["statusCode"] = new OpenApiSchema { Type = "integer", Nullable = true }
                        }
                    },
                    Description = "Details of failed instances"
                },
                ["correlationId"] = new OpenApiSchema { Type = "string", Description = "Correlation ID for tracing" },
                ["instanceId"] = new OpenApiSchema { Type = "string", Description = "Instance that processed the request" },
                ["timestamp"] = new OpenApiSchema { Type = "string", Format = "date-time", Description = "When the error occurred" }
            },
            Required = new HashSet<string> { "errorCode", "message", "successCount", "failureCount", "totalCount", "correlationId", "instanceId", "timestamp" }
        });
    }
}
