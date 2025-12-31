namespace MoodleInstanceBridge.Models.Errors
{
    /// <summary>
    /// Base exception class for standardized error handling
    /// </summary>
    public class StandardException : Exception
    {
        public ErrorCode ErrorCode { get; set; }
        public int StatusCode { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }

        public StandardException(ErrorCode errorCode, string message, int statusCode = 500)
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        public StandardException(ErrorCode errorCode, string message, Exception innerException, int statusCode = 500)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Exception for validation errors
    /// </summary>
    public class ValidationException : StandardException
    {
        public Dictionary<string, string[]> ValidationErrors { get; set; } = new();

        public ValidationException(Dictionary<string, string[]> validationErrors)
            : base(ErrorCode.ValidationError, "One or more validation errors occurred.", 400)
        {
            ValidationErrors = validationErrors;
        }

        public ValidationException(string field, string error)
            : base(ErrorCode.ValidationError, "One or more validation errors occurred.", 400)
        {
            ValidationErrors[field] = new[] { error };
        }
    }

    /// <summary>
    /// Exception for upstream Moodle errors
    /// </summary>
    public class MoodleUpstreamException : StandardException
    {
        public string? MoodleInstanceId { get; set; }
        public int? MoodleStatusCode { get; set; }

        public MoodleUpstreamException(string message, string? moodleInstanceId = null, int? moodleStatusCode = null)
            : base(ErrorCode.MoodleUpstreamError, message, 502)
        {
            MoodleInstanceId = moodleInstanceId;
            MoodleStatusCode = moodleStatusCode;
        }

        public MoodleUpstreamException(string message, Exception innerException, string? moodleInstanceId = null)
            : base(ErrorCode.MoodleUpstreamError, message, innerException, 502)
        {
            MoodleInstanceId = moodleInstanceId;
        }
    }

    /// <summary>
    /// Exception for authentication errors
    /// </summary>
    public class AuthenticationException : StandardException
    {
        public AuthenticationException(string message = "Authentication failed.")
            : base(ErrorCode.AuthenticationError, message, 401)
        {
        }
    }

    /// <summary>
    /// Exception for authorization errors
    /// </summary>
    public class AuthorizationException : StandardException
    {
        public AuthorizationException(string message = "Authorization failed. Insufficient permissions.")
            : base(ErrorCode.AuthorizationError, message, 403)
        {
        }
    }

    /// <summary>
    /// Exception for resource not found
    /// </summary>
    public class NotFoundException : StandardException
    {
        public NotFoundException(string resource)
            : base(ErrorCode.NotFoundError, $"Resource '{resource}' not found.", 404)
        {
        }
    }
}
