using LearningHub.Nhs.Models.Moodle;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Payloads
{
    /// <summary>
    /// Category data from a single Moodle instance
    /// </summary>
    /// <remarks>
    /// Contains categories returned from the core_course_get_categories Moodle web service API.
    /// </remarks>
    public class CategoriesPayload
    {
        /// <summary>
        /// List of categories from this instance
        /// </summary>
        [JsonPropertyName("categories")]
        public List<MoodleCategory> Categories { get; set; } = new();
    }
}
