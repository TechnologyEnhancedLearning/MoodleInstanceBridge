using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MoodleInstanceBridge.HealthChecks
{
    /// <summary>
    /// Health check that includes instance metadata
    /// </summary>
    public class InstanceHealthCheck : IHealthCheck
    {
        private readonly string _instanceId;
        private readonly string _environment;

        public InstanceHealthCheck(IConfiguration configuration)
        {
            _instanceId = configuration["InstanceId"] 
                ?? Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") 
                ?? Environment.MachineName;
            
            _environment = configuration["Environment"] 
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                ?? "Unknown";
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, object>
            {
                { "instanceId", _instanceId },
                { "environment", _environment },
                { "timestamp", DateTime.UtcNow },
                { "machineName", Environment.MachineName }
            };

            return Task.FromResult(
                HealthCheckResult.Healthy(
                    "Instance is healthy",
                    data
                )
            );
        }
    }
}
