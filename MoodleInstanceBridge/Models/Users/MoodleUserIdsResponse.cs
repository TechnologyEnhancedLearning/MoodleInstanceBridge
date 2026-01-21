using System.Text.Json.Serialization;
using LearningHub.Nhs.Models.Moodle;
using MoodleInstanceBridge.Models.Errors;

namespace MoodleInstanceBridge.Models.Users
{
    /// <summary>
    /// Response containing Moodle user IDs per instance for a Learning Hub user
    /// </summary>
    public class MoodleUserIdsResponse
    {
        /// <summary>
        /// List of Moodle user IDs per instance
        /// </summary>
        [JsonPropertyName("moodle_user_ids")]
        public List<MoodleUserIdResult> MoodleUserIds { get; set; } = new();

        /// <summary>
        /// List of errors from instances that failed
        /// </summary>
        [JsonPropertyName("errors")]
        public List<InstanceError> Errors { get; set; } = new();
    }

    /// <summary>
    /// Moodle user ID for a specific instance
    /// </summary>
    public class MoodleUserIdResult
    {
        /// <summary>
        /// Instance short name identifier
        /// </summary>
        [JsonPropertyName("instance")]
        public required string Instance { get; set; }

        /// <summary>
        /// Moodle user ID, or null if user does not exist in this instance
        /// </summary>
        [JsonPropertyName("user_id")]
        public MoodleUser? UserId { get; set; }
    }
}
