using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Models.Moodle
{
    /// <summary>
    /// Moodle user returned from core_user_get_users_by_field
    /// </summary>
    public class MoodleUser
    {
        /// <summary>
        /// Moodle user ID
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// User's username
        /// </summary>
        [JsonPropertyName("username")]
        public string? Username { get; set; }

        /// <summary>
        /// User's email address
        /// </summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        [JsonPropertyName("firstname")]
        public string? FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        [JsonPropertyName("lastname")]
        public string? LastName { get; set; }
    }
}
