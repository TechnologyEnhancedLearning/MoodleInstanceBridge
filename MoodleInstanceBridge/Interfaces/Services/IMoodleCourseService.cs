using LearningHub.Nhs.Models.Moodle;
using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Models.Configuration;

namespace MoodleInstanceBridge.Interfaces.Services
{
    /// <summary>
    /// Service for Moodle course-related operations
    /// </summary>
    public interface IMoodleCourseService
    {
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
