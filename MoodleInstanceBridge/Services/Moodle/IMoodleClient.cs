using MoodleInstanceBridge.Models.Configuration;
using MoodleInstanceBridge.Models.Moodle;

namespace MoodleInstanceBridge.Services.Moodle
{
    /// <summary>
    /// Interface for Moodle Web Service client
    /// </summary>
    public interface IMoodleClient
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
