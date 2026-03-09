using MoodleInstanceBridge.Contracts.Aggregate;
using MoodleInstanceBridge.Contracts.Payloads;

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
        /// <returns>Aggregated response containing categories per instance and any errors</returns>
        Task<AggregateResponse<CategoriesPayload>> GetCategoriesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Search for courses across all enabled Moodle instances by field
        /// </summary>
        /// <param name="field">Field to search by (e.g., "category", "id")</param>
        /// <param name="value">Value to search for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Aggregated response containing courses per instance and any errors</returns>
        Task<AggregateResponse<CoursesPayload>> SearchCoursesAsync(
            string field,
            string value,
            CancellationToken cancellationToken = default);
    }
}
