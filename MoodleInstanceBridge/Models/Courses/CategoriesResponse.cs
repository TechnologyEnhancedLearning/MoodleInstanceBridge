using System.Text.Json.Serialization;
using LearningHub.Nhs.Models.Moodle;
using MoodleInstanceBridge.Contracts.Errors;

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
}
