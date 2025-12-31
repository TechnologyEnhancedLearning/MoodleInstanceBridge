namespace MoodleInstanceBridge.Models.Errors
{
    /// <summary>
    /// Represents a partial failure in multi-instance aggregation
    /// </summary>
    public class PartialFailureResponse : ErrorResponse
    {
        /// <summary>
        /// Number of instances that succeeded
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Number of instances that failed
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// Total number of instances queried
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Details about failed instances
        /// </summary>
        public List<InstanceFailureDetail> FailedInstances { get; set; } = new();

        public PartialFailureResponse()
        {
            ErrorCode = nameof(Models.Errors.ErrorCode.PartialInstanceFailure);
            Message = "Some instances failed to respond.";
        }
    }

    /// <summary>
    /// Details about a specific instance failure
    /// </summary>
    public class InstanceFailureDetail
    {
        /// <summary>
        /// Identifier of the failed instance
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// Error message from the failed instance
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// HTTP status code returned by the instance (if applicable)
        /// </summary>
        public int? StatusCode { get; set; }
    }
}
