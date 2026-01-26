using LearningHub.Nhs.Models.Moodle.API;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Payloads
{
    /// <summary>
    /// Course completion data from all instances
    /// </summary>
    public class UsersPayload
    {
        [JsonPropertyName("users")]
        public MoodleUserResponseModel Users { get; set; } = new();
    }
}
