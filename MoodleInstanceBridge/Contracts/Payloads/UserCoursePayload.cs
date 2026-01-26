using LearningHub.Nhs.Models.Moodle.API;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Payloads
{
    /// <summary>
    /// Certificate data from all instances
    /// </summary>
    public class UserCoursePayload
    {
        /// <summary>
        /// List of courses from all instances
        /// </summary>
        [JsonPropertyName("courses")]
        public List<MoodleCourseResponseModel> Courses { get; set; } = new();

    }
}
