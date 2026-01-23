using MoodleInstanceBridge.Contracts.Errors;

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
            string errorCode;
            string errorMessage;

            switch (ex)
            {
                case System.Text.Json.JsonException jsonEx:
                    errorCode = "INVALID_JSON_RESPONSE";
                    errorMessage =
                        "Moodle instance returned an unexpected response format. " +
                        "The response could not be parsed.";
                    break;

                case TaskCanceledException:
                    errorCode = "TIMEOUT";
                    errorMessage = "Request to Moodle instance timed out";
                    break;

                case HttpRequestException httpEx when httpEx.InnerException is System.Net.Sockets.SocketException:
                    errorCode = "CONNECTION_ERROR";
                    errorMessage = "Failed to connect to Moodle instance";
                    break;

                case HttpRequestException:
                    errorCode = "HTTP_ERROR";
                    errorMessage = "HTTP error occurred while calling Moodle instance";
                    break;

                default:
                    errorCode = "INSTANCE_UNAVAILABLE";
                    errorMessage = "Moodle instance could not be reached";
                    break;
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
