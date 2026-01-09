using Azure.Identity;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Azure.AppConfiguration.AspNetCore;
using Microsoft.EntityFrameworkCore;
using MoodleInstanceBridge.Data;
using MoodleInstanceBridge.HealthChecks;
using MoodleInstanceBridge.Middleware;
using MoodleInstanceBridge.Services.Background;
using MoodleInstanceBridge.Services.Configuration;
using MoodleInstanceBridge.Telemetry;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
// Add Database Context
var connectionString = builder.Configuration.GetConnectionString("LearningHubDbConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string 'LearningHubDbConnection' is not configured. " +
        "Please set it in appsettings.json or environment variables.");
}

builder.Services.AddDbContextFactory<MoodleInstanceContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());
});

// Azure App Configuration
bool appConfigEnabled = false;

builder.Services.AddAzureAppConfiguration();

var appConfigConnection =
    builder.Configuration["AppConfig:ConnectionString"];

if (!string.IsNullOrWhiteSpace(appConfigConnection))
{
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options
            .Connect(appConfigConnection)
            .ConfigureKeyVault(kv =>
            {
                kv.SetCredential(new DefaultAzureCredential());
            })
            .ConfigureRefresh(refresh =>
            {
                refresh
                    .Register("AppSettings:Sentinel", refreshAll: true)
                    .SetRefreshInterval(TimeSpan.FromSeconds(30));
            });
    });

    appConfigEnabled = true;
}


// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableDependencyTrackingTelemetryModule = true;
    options.EnablePerformanceCounterCollectionModule = true;
});

// Register Telemetry Initializer
builder.Services.AddSingleton<ITelemetryInitializer, InstanceTelemetryInitializer>();

// Register configuration services
builder.Services.AddSingleton<IInstanceConfigurationService, InstanceConfigurationService>();
builder.Services.AddHostedService<ConfigurationRefreshService>();


// Register services
builder.Services.AddScoped<MoodleInstanceBridge.Services.AggregationService>();

// Add services
builder.Services.AddControllers(options =>
{
    // Add validation filter globally
    options.Filters.Add<MoodleInstanceBridge.Filters.ValidationExceptionFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    // Suppress default model validation error response to use our custom one
    options.SuppressModelStateInvalidFilter = true;
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<InstanceHealthCheck>("instance_health")
    .AddCheck<ConfigurationHealthCheck>("configuration_health");

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

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature =
            context.Features.Get<IExceptionHandlerPathFeature>();

        var logger = context.RequestServices
            .GetRequiredService<ILogger<Program>>();

        logger.LogError(
            exceptionFeature?.Error,
            "Unhandled exception at {Path}",
            context.Request.Path);

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = new
            {
                code = "INTERNAL_SERVER_ERROR",
                message = "An unexpected error occurred"
            }
        });
    });
});


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
if (appConfigEnabled)
{
    app.UseAzureAppConfiguration();
}
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthorization();

// Map Health Check endpoint
app.MapHealthChecks("/health");

app.MapControllers();
app.Run();
