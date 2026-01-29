using LearningHub.Nhs.Models.Moodle.API;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Payloads
{
    /// <summary>
    /// Course completion status data from a single Moodle instance
    /// </summary>
    /// <remarks>
    /// Contains completion data returned from the core_completion_get_course_completion_status 
    /// Moodle web service API. Includes overall completion state for a specific course.
    /// </remarks>
    public class CourseCompletionStatusPayload
    {
        /// <summary>
        /// Course completion status details including completion state and criteria
        /// </summary>
        [JsonPropertyName("completions")]
        public MoodleCourseCompletionModel Completions { get; set; } = new();
    }
}
