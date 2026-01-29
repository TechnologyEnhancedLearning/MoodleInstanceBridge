using LearningHub.Nhs.Models.Moodle.API;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Payloads
{
    /// <summary>
    /// Recent course access data from a single Moodle instance
    /// </summary>
    /// <remarks>
    /// Contains courses returned from the mylearningservice_get_recent_courses Moodle web service API.
    /// Lists courses the user has recently accessed, typically ordered by last access time.
    /// </remarks>
    public class RecentCoursesPayload
    {
        /// <summary>
        /// List of recently accessed courses with enrollment and progress details
        /// </summary>
        /// <remarks>
        /// Returns an empty list if the user has not accessed any courses recently.
        /// </remarks>
        [JsonPropertyName("courses")]
        public List<MoodleEnrolledCourseResponseModel> Courses { get; set; } = new();
    }
}
