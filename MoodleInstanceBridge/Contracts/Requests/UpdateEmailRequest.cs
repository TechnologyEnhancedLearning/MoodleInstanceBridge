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
}
