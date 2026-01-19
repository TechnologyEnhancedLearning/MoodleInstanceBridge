using LearningHub.Nhs.Models.Moodle;
using MoodleInstanceBridge.Models.Configuration;

namespace MoodleInstanceBridge.Interfaces.Services
{
    /// <summary>
    /// Service for Moodle user-related operations
    /// </summary>
    public interface IMoodleUserService
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
    }
}
