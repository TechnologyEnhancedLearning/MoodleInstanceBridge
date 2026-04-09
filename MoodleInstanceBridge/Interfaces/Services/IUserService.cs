using MoodleInstanceBridge.Contracts.Aggregate;
using MoodleInstanceBridge.Contracts.Payloads;
using MoodleInstanceBridge.Contracts.Requests;
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
        /// <param name="months">Number of months to look back for recent courses</param>
        /// <param name="statusfilter">Optional filter for course status (e.g. "inprogress", "completed")</param>
        /// <param name="search">Optional search text to filter courses by name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing recent courses from all instances and any errors</returns>
        Task<AggregateResponse<RecentCoursesPayload>> GetRecentCoursesAsync(
            UserIdsRequest userIdsRequest,
            string months,
            string statusfilter,
            string search,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user certificates across all enabled instances
        /// </summary>
        /// <param name="userIdsRequest">Map of instance IDs to user IDs</param>
        /// <param name="filterText">Optional filter text to search for in certificate names</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing user certificates from all instances and any errors</returns>
        Task<AggregateResponse<UserCertificatesPayload>> GetUserCertificatesAsync(
            UserIdsRequest userIdsRequest,
            string filterText,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a user's email address across all enabled Moodle instances
        /// </summary>
        /// <remarks>
        /// Looks up the user by old email across all instances, then updates the email
        /// in each instance where the user exists. Instances where the user is not found
        /// are skipped without error. Partial failures do not block updates in other instances.
        /// </remarks>
        /// <param name="request">Request containing old and new email addresses</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response with per-instance update status and any errors</returns>
        Task<UpdateEmailResponse> UpdateUserEmailAsync(
            UpdateEmailRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get badges awarded to users across specified Moodle instances
        /// </summary>
        /// <param name="userIdsRequest">Map of instance IDs to user IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing user badges from all instances and any errors</returns>
        Task<AggregateResponse<UserBadgesPayload>> GetUserBadgesAsync(
            UserIdsRequest userIdsRequest,
            CancellationToken cancellationToken = default);
    }
}
