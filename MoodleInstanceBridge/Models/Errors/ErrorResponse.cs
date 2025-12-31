using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Models.Errors
{
    /// <summary>
    /// Standardized error response model for consistent error handling
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Stable error code for programmatic handling
        /// </summary>
        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable error message
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Optional technical details for debugging (not shown in production)
        /// </summary>
        [JsonPropertyName("detail")]
        public string? Detail { get; set; }

        /// <summary>
        /// Correlation ID for tracing the request
        /// </summary>
        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Instance ID that processed the request
        /// </summary>
        [JsonPropertyName("instanceId")]
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the error occurred
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional metadata about the error
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
