using MoodleInstanceBridge.Contracts.Aggregate;
using MoodleInstanceBridge.Contracts.Payloads;
using MoodleInstanceBridge.Models.Courses;
using MoodleInstanceBridge.Models.Users;

namespace MoodleInstanceBridge.Interfaces.Services
{
    /// <summary>
    /// Service for looking up Moodle user IDs across instances
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get Moodle user IDs for a given email across all enabled instances
        /// </summary>
        /// <param name="email">Email address to look up</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing user IDs per instance and any errors</returns>
        Task<MoodleUserIdsResponse> GetMoodleUserIdsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get courses for a user across all enabled instances
        /// </summary>
        /// <param name="userIdsRequest">Map of instance IDs to user IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing user courses from all instances and any errors</returns>
        Task<AggregateResponse<UserCoursePayload>> GetUserCoursesAsync(
            UserIdsRequest userIdsRequest,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get course completion status across all enabled instances
        /// </summary>
        /// <param name="userIdsRequest">Map of instance IDs to user IDs</param>
        /// <param name="courseId">Course ID to check completion for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing course completion data from all instances and any errors</returns>
        Task<AggregateResponse<CourseCompletionStatusPayload>> GetCourseCompletionStatusAsync(
            UserIdsRequest userIdsRequest,
            int courseId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user data across all enabled instances
        /// </summary>
        /// <param name="userIdsRequest">Map of instance IDs to user IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing user data from all instances and any errors</returns>
        Task<AggregateResponse<UsersPayload>> GetUsersAsync(
            UserIdsRequest userIdsRequest,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get recent courses across all enabled instances
        /// </summary>
        /// <param name="userIdsRequest">Map of instance IDs to user IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing recent courses from all instances and any errors</returns>
        Task<AggregateResponse<RecentCoursesPayload>> GetRecentCoursesAsync(
            UserIdsRequest userIdsRequest,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user certificates across all enabled instances
        /// </summary>
        /// <param name="userIdsRequest">Map of instance IDs to user IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing user certificates from all instances and any errors</returns>
        Task<AggregateResponse<UserCertificatesPayload>> GetUserCertificatesAsync(
            UserIdsRequest userIdsRequest,
            CancellationToken cancellationToken = default);
    }
}
