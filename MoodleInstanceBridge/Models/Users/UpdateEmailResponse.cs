using System.Text.Json.Serialization;
using MoodleInstanceBridge.Contracts.Errors;

namespace MoodleInstanceBridge.Models.Users
{
    /// <summary>
    /// Response from the update-email operation across all Moodle instances
    /// </summary>
    public class UpdateEmailResponse
    {
        /// <summary>
        /// Instances where the email was successfully updated
        /// </summary>
        [JsonPropertyName("results")]
        public List<UpdateEmailResult> Results { get; set; } = new();

        /// <summary>
        /// Instances where the update failed or the user was not found
        /// </summary>
        [JsonPropertyName("errors")]
        public List<InstanceError> Errors { get; set; } = new();
    }

    /// <summary>
    /// Per-instance result of an email update operation
    /// </summary>
    public class UpdateEmailResult
    {
        /// <summary>
        /// Instance short name identifier
        /// </summary>
        [JsonPropertyName("instance")]
        public required string Instance { get; set; }

        /// <summary>
        /// Operation status (e.g. "updated")
        /// </summary>
        [JsonPropertyName("status")]
        public required string Status { get; set; }
    }
}
