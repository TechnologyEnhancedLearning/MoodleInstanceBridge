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

        /// <summary>
        /// Get courses for a user using core_enrol_get_users_courses web service
        /// </summary>
        /// <param name="config">Moodle instance configuration</param>
        /// <param name="userId">Moodle user ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing user's enrolled courses</returns>
        Task<List<MoodleCourseResponseModel>> GetUserCoursesAsync(
            MoodleInstanceConfig config,
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get course completion status using core_completion_get_course_completion_status web service
        /// </summary>
        /// <param name="config">Moodle instance configuration</param>
        /// <param name="userId">Moodle user ID</param>
        /// <param name="courseId">Moodle course ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Course completion status</returns>
        Task<MoodleCourseCompletionModel> GetCourseCompletionStatusAsync(
            MoodleInstanceConfig config,
            int userId,
            int courseId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get users using core_user_get_users web service
        /// </summary>
        /// <param name="config">Moodle instance configuration</param>
        /// <param name="userId">Moodle user ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User data</returns>
        Task<MoodleUserResponseModel> GetUsersAsync(
            MoodleInstanceConfig config,
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get recent courses using mylearningservice_get_recent_courses web service
        /// </summary>
        /// <param name="config">Moodle instance configuration</param>
        /// <param name="userId">Moodle user ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Recent courses</returns>
        Task<List<MoodleEnrolledCourseResponseModel>> GetRecentCoursesAsync(
            MoodleInstanceConfig config,
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user certificates using mylearningservice_get_user_certificates web service
        /// </summary>
        /// <param name="config">Moodle instance configuration</param>
        /// <param name="userId">Moodle user ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User certificates</returns>
        Task<List<MoodleUserCertificateResponseModel>> GetUserCertificatesAsync(
            MoodleInstanceConfig config,
            int userId,
            CancellationToken cancellationToken = default);
    }
}
