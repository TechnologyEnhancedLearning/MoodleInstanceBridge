using MoodleInstanceBridge.Models.Errors;
using MoodleInstanceBridge.Models.Users;

namespace MoodleInstanceBridge.Controllers.Helpers
{
    /// <summary>
    /// Helper methods for common request validation patterns in controllers
    /// </summary>
    public static class RequestValidationHelper
    {
        /// <summary>
        /// Validates a UserIdsRequest to ensure it contains required data
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <exception cref="ValidationException">Thrown if validation fails</exception>
        public static void ValidateUserIdsRequest(UserIdsRequest? request)
        {
            if (request?.UserIds == null || !request.UserIds.Any())
            {
                throw new ValidationException("userIds", "UserIds map is required and must contain at least one entry.");
            }
        }
    }
}