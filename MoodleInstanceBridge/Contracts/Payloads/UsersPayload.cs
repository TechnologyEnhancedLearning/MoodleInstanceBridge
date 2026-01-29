using LearningHub.Nhs.Models.Moodle.API;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Payloads
{
    /// <summary>
    /// User profile and account data from a single Moodle instance
    /// </summary>
    /// <remarks>
    /// Contains user information returned from the core_user_get_users Moodle web service API.
    /// </remarks>
    public class UsersPayload
    {
        /// <summary>
        /// User profile data including name, email, preferences, and custom fields
        /// </summary>
        [JsonPropertyName("users")]
        public MoodleUserResponseModel Users { get; set; } = new();
    }
}
