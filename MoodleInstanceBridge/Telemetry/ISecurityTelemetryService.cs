namespace MoodleInstanceBridge.Telemetry
{
    public interface ISecurityTelemetryService
    {
        void TrackAuthSuccess(string method, string path, string? user = null, string? client = null);
        void TrackAuthFailure(string failureType, string path, string? ip = null, string? reason = null);
        void TrackJwtFailure(string error, string path);
    }
}
