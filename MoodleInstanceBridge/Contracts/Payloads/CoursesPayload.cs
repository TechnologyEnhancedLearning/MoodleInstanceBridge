using LearningHub.Nhs.Models.Moodle.API;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Payloads
{
    /// <summary>
    /// Course search result data from a single Moodle instance
    /// </summary>
    /// <remarks>
    /// Contains courses returned from the core_course_get_courses_by_field Moodle web service API.
    /// </remarks>
    public class CoursesPayload
    {
        /// <summary>
        /// Courses matching the search criteria from this instance
        /// </summary>
        [JsonPropertyName("courses")]
        public MoodleCoursesResponseModel Courses { get; set; } = new();
    }
}
