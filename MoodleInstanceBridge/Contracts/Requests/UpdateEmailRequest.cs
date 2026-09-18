using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Requests
{
    /// <summary>
    /// Request model for updating a user's email address across all Moodle instances
    /// </summary>
    public class UpdateEmailRequest
    {
        /// <summary>
        /// The user's current email address (used to locate the user across instances)
        /// </summary>
        [Required]
        [JsonPropertyName("oldEmail")]
        public string OldEmail { get; set; } = string.Empty;

        /// <summary>
        /// The new email address to set for the user
        /// </summary>
        [Required]
        [JsonPropertyName("newEmail")]
        public string NewEmail { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for enrolling a Learning Hub user onto a Moodle course in a specific instance
    /// </summary>
    public class EnrolmentRequest
    {
        /// <summary>
        /// The target Moodle instance short name
        /// </summary>
        [Required]
        [JsonPropertyName("instanceId")]
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// The Moodle course ID to enrol the user onto
        /// </summary>
        [Required]
        [JsonPropertyName("courseId")]
        public int CourseId { get; set; }

        /// <summary>
        /// The Learning Hub user's email address, used by MIB to locate the Moodle user
        /// </summary>
        [Required]
        [JsonPropertyName("userEmail")]
        public string UserEmail { get; set; } = string.Empty;
    }
}
