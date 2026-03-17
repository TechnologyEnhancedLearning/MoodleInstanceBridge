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
        /// Search for courses across all enabled Moodle instances by field,
        /// or from a specific instance when <paramref name="instance"/> is provided.
        /// </summary>
        /// <param name="field">Field to search by (e.g., "category", "id")</param>
        /// <param name="value">Value to search for</param>
        /// <param name="instance">
        /// Optional short name of a specific Moodle instance to query.
        /// When <c>null</c> or empty all enabled instances are queried.
        /// </param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Aggregated response containing courses per instance and any errors</returns>
        Task<AggregateResponse<CoursesPayload>> SearchCoursesAsync(
            string field,
            string value,
            string? instance = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get subcategories of a given category across all enabled Moodle instances,
        /// or from a specific instance when <paramref name="instance"/> is provided.
        /// </summary>
        /// <param name="categoryId">Parent category ID to retrieve subcategories for</param>
        /// <param name="instance">
        /// Optional short name of a specific Moodle instance to query.
        /// When <c>null</c> or empty all enabled instances are queried.
        /// </param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Aggregated response containing subcategories per instance and any errors</returns>
        Task<AggregateResponse<SubCategoriesPayload>> GetSubCategoriesAsync(
            int categoryId,
            string? instance = null,
            CancellationToken cancellationToken = default);
    }
}
