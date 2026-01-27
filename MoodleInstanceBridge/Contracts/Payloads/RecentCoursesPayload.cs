using LearningHub.Nhs.Models.Moodle.API;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Payloads
{
    public class RecentCoursesPayload
    {
        /// <summary>
        /// List of courses from all instances
        /// </summary>
        [JsonPropertyName("courses")]
        public List<MoodleEnrolledCourseResponseModel> Courses { get; set; } = new();
    }
}
