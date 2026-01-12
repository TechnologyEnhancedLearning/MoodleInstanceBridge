using MoodleInstanceBridge.Models.Users;

namespace MoodleInstanceBridge.Services.Users
{
    /// <summary>
    /// Service for looking up Moodle user IDs across instances
    /// </summary>
    public interface IUserLookupService
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
    }
}
