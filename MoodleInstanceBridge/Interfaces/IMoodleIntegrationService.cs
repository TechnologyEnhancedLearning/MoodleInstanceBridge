using LearningHub.Nhs.Models.Moodle;
using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Models.Configuration;

namespace MoodleInstanceBridge.Interfaces
{
    /// <summary>
    /// Interface for Moodle Web Service client
    /// </summary>
    public interface IMoodleIntegrationService
    {
        /// <summary>
        /// Get users by field using core_user_get_users_by_field web service
        /// </summary>
        /// <param name="config">Moodle instance configuration</param>
        /// <param name="field">Field to search by (e.g., "email", "username")</param>
        /// <param name="value">Value to search for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of matching Moodle users</returns>
        Task<List<MoodleUser>> GetUsersByFieldAsync(
            MoodleInstanceConfig config,
            string field,
            string value,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get categories using core_course_get_categories web service
        /// </summary>
        /// <param name="config">Moodle instance configuration</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of Moodle categories</returns>
        Task<List<MoodleCategory>> GetCategoriesAsync(
            MoodleInstanceConfig config,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get courses by field using core_course_get_courses_by_field web service
        /// </summary>
        /// <param name="config">Moodle instance configuration</param>
        /// <param name="field">Field to search by (e.g., "category", "id")</param>
        /// <param name="value">Value to search for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing list of matching courses</returns>
        Task<MoodleCoursesResponseModel> GetCoursesByFieldAsync(
            MoodleInstanceConfig config,
            string field,
            string value,
            CancellationToken cancellationToken = default);
    }
}
