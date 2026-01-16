using System.Text.Json.Serialization;
using LearningHub.Nhs.Models.Moodle;
using MoodleInstanceBridge.Models.Users;

namespace MoodleInstanceBridge.Models.Courses
{
    /// <summary>
    /// Response containing categories from all Moodle instances
    /// </summary>
    public class CategoriesResponse
    {
        /// <summary>
        /// List of categories from all instances
        /// </summary>
        [JsonPropertyName("categories")]
        public List<MoodleCategory> Categories { get; set; } = new();

        /// <summary>
        /// List of errors from instances that failed
        /// </summary>
        [JsonPropertyName("errors")]
        public List<InstanceError> Errors { get; set; } = new();
    }

    /// <summary>
    /// Category result with source instance information
    /// </summary>
    public class CategoryResult
    {
        /// <summary>
        /// Source Moodle instance short name
        /// </summary>
        [JsonPropertyName("source_instance")]
        public required string SourceInstance { get; set; }

        /// <summary>
        /// Category ID
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Category description
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Parent category ID (0 if top-level)
        /// </summary>
        [JsonPropertyName("parent")]
        public int Parent { get; set; }

        /// <summary>
        /// Number of courses in this category
        /// </summary>
        [JsonPropertyName("course_count")]
        public int CourseCount { get; set; }

        /// <summary>
        /// Whether category is visible
        /// </summary>
        [JsonPropertyName("visible")]
        public bool Visible { get; set; }

        /// <summary>
        /// Category depth level
        /// </summary>
        [JsonPropertyName("depth")]
        public int Depth { get; set; }

        /// <summary>
        /// Category path (slash-separated IDs)
        /// </summary>
        [JsonPropertyName("path")]
        public string? Path { get; set; }
    }
}
