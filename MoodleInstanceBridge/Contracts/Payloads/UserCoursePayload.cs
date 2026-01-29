using LearningHub.Nhs.Models.Moodle.API;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Payloads
{
    /// <summary>
    /// User course enrollment data from a single Moodle instance
    /// </summary>
    /// <remarks>
    /// Contains courses returned from the core_enrol_get_users_courses Moodle web service API.
    /// Each course includes enrollment details, progress, and visibility information.
    /// </remarks>
    public class UserCoursePayload
    {
        /// <summary>
        /// List of courses the user is enrolled in
        /// </summary>
        /// <remarks>
        /// Returns an empty list if the user has no course enrollments in this instance.
        /// </remarks>
        [JsonPropertyName("courses")]
        public List<MoodleCourseResponseModel> Courses { get; set; } = new();
    }
}
