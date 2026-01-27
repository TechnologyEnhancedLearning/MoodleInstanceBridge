using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Models.Users
{
    /// <summary>
    /// Request containing user IDs per Moodle instance
    /// </summary>
    public class UserIdsRequest
    {
        /// <summary>
        /// Map of instance short name to Moodle user ID
        /// </summary>
        [JsonPropertyName("userIds")]
        public Dictionary<string, int> UserIds { get; set; } = new();
    }
}