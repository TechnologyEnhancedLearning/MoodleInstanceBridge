using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
namespace MoodleInstanceBridge.Telemetry
{
    public class SecurityTelemetryService : ISecurityTelemetryService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<SecurityTelemetryService> _logger;

        public SecurityTelemetryService(
            TelemetryClient telemetryClient,
            ILogger<SecurityTelemetryService> logger)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        public void TrackAuthSuccess(string method, string path, string? user = null, string? client = null)
        {
            var properties = new Dictionary<string, string>
            {
                ["Method"] = method,
                ["Path"] = path,
                ["User"] = user ?? "unknown",
                ["Client"] = client ?? "unknown"
            };

            _logger.LogInformation("Authentication success via {Method} on {Path}", method, path);
            _telemetryClient.TrackEvent("AuthSuccess", properties);
        }

        public void TrackAuthFailure(string failureType, string path, string? ip = null, string? reason = null)
        {
            var properties = new Dictionary<string, string>
            {
                ["FailureType"] = failureType,
                ["Path"] = path,
                ["IP"] = ip ?? "unknown",
                ["Reason"] = reason ?? "not specified"
            };

            _logger.LogWarning(
                "Authentication failed: {FailureType} | Path: {Path} | IP: {IP}",
                failureType, path, ip);

            _telemetryClient.TrackEvent("AuthFailure", properties);
            _telemetryClient.GetMetric("FailedAuthenticationCount").TrackValue(1);
        }

        public void TrackJwtFailure(string error, string path)
        {
            var properties = new Dictionary<string, string>
            {
                ["Error"] = error,
                ["Path"] = path
            };

            _logger.LogWarning("JWT authentication failed on {Path}: {Error}", path, error);
            _telemetryClient.TrackEvent("JWT_Authentication_Failed", properties);
        }
    }
}
