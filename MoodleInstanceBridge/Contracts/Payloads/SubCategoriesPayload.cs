using LearningHub.Nhs.Models.Moodle.API;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Payloads
{
    /// <summary>
    /// Subcategory data from a single Moodle instance
    /// </summary>
    /// <remarks>
    /// Contains subcategories returned from the core_course_get_categories Moodle web service API,
    /// filtered to include only direct children of the requested parent category.
    /// </remarks>
    public class SubCategoriesPayload
    {
        /// <summary>
        /// List of subcategories from this instance
        /// </summary>
        [JsonPropertyName("subcategories")]
        public List<MoodleSubCategoryResponseModel> SubCategories { get; set; } = new();
    }
}
