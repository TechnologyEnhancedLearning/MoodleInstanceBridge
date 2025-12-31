using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.ApplicationInsights.Extensibility;
using MoodleInstanceBridge.Telemetry;
using MoodleInstanceBridge.Middleware;
using MoodleInstanceBridge.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableDependencyTrackingTelemetryModule = true;
    options.EnablePerformanceCounterCollectionModule = true;
});

// Register Telemetry Initializer
builder.Services.AddSingleton<ITelemetryInitializer, InstanceTelemetryInitializer>();

// Add services
builder.Services.AddControllers();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<InstanceHealthCheck>("instance_health");

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Explorer (required for Swagger)
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<SwaggerConfig>();

var app = builder.Build();

// Version provider
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Middleware - Error handling should be first
app.UseErrorHandling();

// Request timing middleware
app.UseRequestTiming();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant()
        );
    }
});

app.UseHttpsRedirection();
app.UseAuthorization();

// Map Health Check endpoint
app.MapHealthChecks("/health");

app.MapControllers();
app.Run();
