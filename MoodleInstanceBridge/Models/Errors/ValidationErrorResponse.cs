namespace MoodleInstanceBridge.Models.Errors
{
    /// <summary>
    /// Represents a validation error with field-specific details
    /// </summary>
    public class ValidationErrorResponse : ErrorResponse
    {
        /// <summary>
        /// Collection of validation errors per field
        /// </summary>
        public Dictionary<string, string[]> Errors { get; set; } = new();

        public ValidationErrorResponse()
        {
            ErrorCode = nameof(Models.Errors.ErrorCode.ValidationError);
            Message = "One or more validation errors occurred.";
        }
    }
}
