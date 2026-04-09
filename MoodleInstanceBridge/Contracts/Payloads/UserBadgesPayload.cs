using MoodleInstanceBridge.Models.Moodle;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Payloads
{
    /// <summary>
    /// Badge data for a user from a single Moodle instance
    /// </summary>
    /// <remarks>
    /// Contains badges returned from the core_badges_get_user_badges Moodle web service API.
    /// </remarks>
    public class UserBadgesPayload
    {
        /// <summary>
        /// List of badges awarded to the user
        /// </summary>
        /// <remarks>
        /// Returns an empty list if the user has no badges in this instance.
        /// </remarks>
        [JsonPropertyName("badges")]
        public List<MoodleUserBadgeResponseModel> Badges { get; set; } = new();
    }
}
