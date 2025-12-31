using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace MoodleInstanceBridge.Telemetry
{
    /// <summary>
    /// Enriches Application Insights telemetry with instance-specific metadata
    /// </summary>
    public class InstanceTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _instanceId;
        private readonly string _environment;

        public InstanceTelemetryInitializer(IConfiguration configuration)
        {
            _instanceId = configuration["InstanceId"] 
                ?? Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") 
                ?? Environment.MachineName;
            
            _environment = configuration["Environment"] 
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                ?? "Unknown";
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleInstance = _instanceId;
            telemetry.Context.GlobalProperties["InstanceId"] = _instanceId;
            telemetry.Context.GlobalProperties["Environment"] = _environment;
        }
    }
}
