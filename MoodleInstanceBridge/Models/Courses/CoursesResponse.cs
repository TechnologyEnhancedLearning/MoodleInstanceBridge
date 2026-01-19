using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Models.Errors;
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

    /// <summary>
    /// Course result with source instance information
    /// </summary>
    public class CourseResult
    {
        /// <summary>
        /// Source Moodle instance short name
        /// </summary>
        [JsonPropertyName("source_instance")]
        public required string SourceInstance { get; set; }

        /// <summary>
        /// Course ID
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Course full name
        /// </summary>
        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }

        /// <summary>
        /// Course short name
        /// </summary>
        [JsonPropertyName("short_name")]
        public string? ShortName { get; set; }

        /// <summary>
        /// Category ID
        /// </summary>
        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        /// <summary>
        /// Course summary
        /// </summary>
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        /// <summary>
        /// Course start date (Unix timestamp)
        /// </summary>
        [JsonPropertyName("start_date")]
        public long StartDate { get; set; }

        /// <summary>
        /// Course end date (Unix timestamp)
        /// </summary>
        [JsonPropertyName("end_date")]
        public long EndDate { get; set; }

        /// <summary>
        /// Whether course is visible
        /// </summary>
        [JsonPropertyName("visible")]
        public bool Visible { get; set; }

        /// <summary>
        /// Course format (e.g., topics, weeks)
        /// </summary>
        [JsonPropertyName("format")]
        public string? Format { get; set; }

        /// <summary>
        /// Course language
        /// </summary>
        [JsonPropertyName("lang")]
        public string? Lang { get; set; }
    }
}
