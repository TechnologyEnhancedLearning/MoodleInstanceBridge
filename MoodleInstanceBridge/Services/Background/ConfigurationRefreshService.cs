using MoodleInstanceBridge.Services.Configuration;

namespace MoodleInstanceBridge.Services.Background
{
    /// <summary>
    /// Background service for periodic configuration refresh
    /// </summary>
    public class ConfigurationRefreshService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationRefreshService> _logger;
        private readonly TimeSpan _refreshInterval;

        public ConfigurationRefreshService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<ConfigurationRefreshService> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;

            // Default refresh interval is 5 minutes
            var intervalMinutes = _configuration.GetValue<int>("InstanceConfiguration:RefreshIntervalMinutes", 5);
            _refreshInterval = TimeSpan.FromMinutes(intervalMinutes);

            _logger.LogInformation(
                "Configuration refresh service initialized with interval: {Interval} minutes",
                intervalMinutes
            );
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Configuration refresh service started");

            // Initial load
            await RefreshConfigurationAsync(stoppingToken);

            // Periodic refresh
            using var timer = new PeriodicTimer(_refreshInterval);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await timer.WaitForNextTickAsync(stoppingToken);
                    await RefreshConfigurationAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in configuration refresh service");
                }
            }

            _logger.LogInformation("Configuration refresh service stopped");
        }

        private async Task RefreshConfigurationAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var configService = scope.ServiceProvider.GetRequiredService<IInstanceConfigurationService>();

                _logger.LogInformation("Starting scheduled configuration refresh");
                var results = await configService.RefreshConfigurationsAsync(cancellationToken);

                var validCount = results.Count(r => r.IsValid);
                var invalidCount = results.Count(r => !r.IsValid);

                _logger.LogInformation(
                    "Scheduled configuration refresh completed: {ValidCount} valid, {InvalidCount} invalid",
                    validCount,
                    invalidCount
                );

                // Log errors for invalid configurations
                foreach (var result in results.Where(r => !r.IsValid))
                {
                    _logger.LogError(
                        "Configuration validation failed for instance {InstanceShortName}: {Errors}",
                        result.InstanceShortName,
                        string.Join(", ", result.Errors)
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh configuration");
            }
        }
    }
}
