using MoodleInstanceBridge.Models.Errors;

namespace MoodleInstanceBridge.Services
{
    /// <summary>
    /// Helper class for creating InstanceError objects from exceptions
    /// </summary>
    public static class InstanceErrorHelper
    {
        /// <summary>
        /// Creates an InstanceError from an exception with appropriate error code and message
        /// </summary>
        /// <param name="shortName">Instance short name</param>
        /// <param name="ex">Exception that occurred</param>
        /// <returns>InstanceError with appropriate error code and message</returns>
        public static InstanceError CreateFromException(string shortName, Exception ex)
        {
            string errorCode = "INSTANCE_UNAVAILABLE";
            string errorMessage = "Moodle instance could not be reached";

            if (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            {
                errorCode = "TIMEOUT";
                errorMessage = "Request to Moodle instance timed out";
            }
            else if (ex.Message.Contains("connect", StringComparison.OrdinalIgnoreCase))
            {
                errorCode = "CONNECTION_ERROR";
                errorMessage = "Failed to connect to Moodle instance";
            }

            return new InstanceError
            {
                Instance = shortName,
                Code = errorCode,
                Message = errorMessage
            };
        }

        /// <summary>
        /// Creates a configuration error for when instance config is not found
        /// </summary>
        /// <param name="shortName">Instance short name</param>
        /// <returns>InstanceError for configuration not found</returns>
        public static InstanceError CreateConfigurationError(string shortName)
        {
            return new InstanceError
            {
                Instance = shortName,
                Code = "CONFIGURATION_ERROR",
                Message = "Instance configuration not found"
            };
        }
    }
}
