using Microsoft.Extensions.Diagnostics.HealthChecks;
using MoodleInstanceBridge.Interfaces;

namespace MoodleInstanceBridge.HealthChecks
{
    /// <summary>
    /// Health check for Moodle instance configurations
    /// </summary>
    public class ConfigurationHealthCheck : IHealthCheck
    {
        private readonly IInstanceConfigurationService _configurationService;
        private readonly ILogger<ConfigurationHealthCheck> _logger;

        public ConfigurationHealthCheck(
            IInstanceConfigurationService configurationService,
            ILogger<ConfigurationHealthCheck> logger)
        {
            _configurationService = configurationService;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var configurations = await _configurationService.GetAllConfigurationsAsync(cancellationToken);
                var validationResults = _configurationService.LastValidationResults;
                var lastRefresh = _configurationService.LastRefreshTime;

                var data = new Dictionary<string, object>
                {
                    { "configurationCount", configurations.Count },
                    { "lastRefreshTime", lastRefresh?.ToString("o") ?? "Never" },
                    { "validConfigurationCount", validationResults.Count(r => r.IsValid) },
                    { "invalidConfigurationCount", validationResults.Count(r => !r.IsValid) }
                };

                // Collect invalid configurations
                var invalidConfigs = validationResults.Where(r => !r.IsValid).ToList();
                if (invalidConfigs.Any())
                {
                    data["invalidConfigurations"] = invalidConfigs.Select(v => new
                    {
                        instanceShortName = v.InstanceShortName,
                        errors = v.Errors
                    }).ToList();
                }

                // Determine health status
                if (!lastRefresh.HasValue)
                {
                    return HealthCheckResult.Degraded(
                        "Configuration has never been refreshed",
                        data: data
                    );
                }

                if (configurations.Count == 0)
                {
                    return HealthCheckResult.Unhealthy(
                        "No valid Moodle instance configurations available",
                        data: data
                    );
                }

                if (invalidConfigs.Any())
                {
                    return HealthCheckResult.Degraded(
                        $"{invalidConfigs.Count} configuration(s) failed validation",
                        data: data
                    );
                }

                return HealthCheckResult.Healthy(
                    $"{configurations.Count} configuration(s) loaded successfully",
                    data: data
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for configurations");
                return HealthCheckResult.Unhealthy(
                    "Failed to check configuration health",
                    ex
                );
            }
        }
    }
}
