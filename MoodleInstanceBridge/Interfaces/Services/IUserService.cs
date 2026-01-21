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
        /// <param name="userId">Moodle user ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing user courses from all instances and any errors</returns>
        //Task<UserCoursesResponse> GetUserCoursesAsync(
        //    int userId,
        //    CancellationToken cancellationToken = default);
    }
}
