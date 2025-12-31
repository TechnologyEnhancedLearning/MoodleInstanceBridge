namespace MoodleInstanceBridge.Models.Errors
{
    /// <summary>
    /// Standardized error codes for predictable error handling
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// An unexpected internal server error occurred
        /// </summary>
        InternalServerError,

        /// <summary>
        /// Request validation failed
        /// </summary>
        ValidationError,

        /// <summary>
        /// Authentication failed or missing credentials
        /// </summary>
        AuthenticationError,

        /// <summary>
        /// Authorization failed - insufficient permissions
        /// </summary>
        AuthorizationError,

        /// <summary>
        /// Requested resource was not found
        /// </summary>
        NotFoundError,

        /// <summary>
        /// Upstream Moodle instance returned an error
        /// </summary>
        MoodleUpstreamError,

        /// <summary>
        /// One or more Moodle instances failed in aggregation
        /// </summary>
        PartialInstanceFailure,

        /// <summary>
        /// All Moodle instances failed in aggregation
        /// </summary>
        AllInstancesFailure,

        /// <summary>
        /// Request timeout occurred
        /// </summary>
        TimeoutError,

        /// <summary>
        /// Service is temporarily unavailable
        /// </summary>
        ServiceUnavailable,

        /// <summary>
        /// Invalid or malformed request
        /// </summary>
        BadRequest
    }
}
