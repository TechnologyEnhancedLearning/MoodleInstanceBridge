using LearningHub.Nhs.Models.Moodle;
using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Models.Courses;

namespace MoodleInstanceBridge.Interfaces.Services
{
    /// <summary>
    /// Service for looking up course data across Moodle instances
    /// </summary>
    public interface ICourseService
    {
        /// <summary>
        /// Get all categories from all enabled Moodle instances
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing categories from all instances and any errors</returns>
        Task<CategoriesResponse> GetCategoriesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Search for courses across all enabled Moodle instances by field
        /// </summary>
        /// <param name="field">Field to search by (e.g., "category", "id")</param>
        /// <param name="value">Value to search for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing courses from all instances and any errors</returns>
        Task<CoursesResponse> SearchCoursesAsync(
            string field,
            string value,
            CancellationToken cancellationToken = default);
    }
}
