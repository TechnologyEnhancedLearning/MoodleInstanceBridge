using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Contracts.Errors;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Models.Courses
{
    /// <summary>
    /// Response containing courses from all Moodle instances
    /// </summary>
    public class CoursesResponse
    {
        /// <summary>
        /// List of courses from all instances
        /// </summary>
        [JsonPropertyName("courses")]
        public MoodleCoursesResponseModel Courses { get; set; } = new();

        /// <summary>
        /// List of errors from instances that failed
        /// </summary>
        [JsonPropertyName("errors")]
        public List<InstanceError> Errors { get; set; } = new();
    }
}
