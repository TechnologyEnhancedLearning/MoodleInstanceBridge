using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Models.Errors
{
    /// <summary>
    /// Error detail for a failed instance
    /// </summary>
    public class InstanceError
    {
        /// <summary>
        /// Instance short name identifier
        /// </summary>
        [JsonPropertyName("instance")]
        public required string Instance { get; set; }

        /// <summary>
        /// Error code
        /// </summary>
        [JsonPropertyName("code")]
        public required string Code { get; set; }

        /// <summary>
        /// Human-readable error message
        /// </summary>
        [JsonPropertyName("message")]
        public required string Message { get; set; }
    }
}
