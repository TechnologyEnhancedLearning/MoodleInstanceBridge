using MoodleInstanceBridge.Models.Errors;
using System.Collections.Concurrent;

namespace MoodleInstanceBridge.Services
{
    /// <summary>
    /// Service to handle multi-instance aggregation with proper error handling
    /// </summary>
    public class AggregationService
    {
        private readonly ILogger<AggregationService> _logger;

        public AggregationService(ILogger<AggregationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Aggregates results from multiple instances with error tracking
        /// </summary>
        /// <typeparam name="TResult">The type of result expected from each instance</typeparam>
        /// <param name="instanceTasks">Dictionary of instance IDs to their async tasks</param>
        /// <param name="allowPartialFailure">Whether to allow partial failures (default: true)</param>
        /// <returns>Aggregation result with success/failure tracking</returns>
        public async Task<AggregationResult<TResult>> AggregateAsync<TResult>(
            Dictionary<string, Task<TResult>> instanceTasks,
            bool allowPartialFailure = true)
        {
            var result = new AggregationResult<TResult>();
            var tasks = instanceTasks.Select(kvp => ExecuteWithTracking(kvp.Key, kvp.Value, result));

            await Task.WhenAll(tasks);

            // Log aggregation results
            _logger.LogInformation(
                "Aggregation completed: {SuccessCount} succeeded, {FailureCount} failed out of {TotalCount} instances",
                result.SuccessCount,
                result.FailureCount,
                result.TotalCount
            );

            // Throw exception if all instances failed
            if (result.FailureCount == result.TotalCount)
            {
                throw new StandardException(
                    ErrorCode.AllInstancesFailure,
                    "All instances failed to respond.",
                    503
                );
            }

            // Throw exception if partial failure is not allowed and some instances failed
            if (!allowPartialFailure && result.FailureCount > 0)
            {
                throw CreatePartialFailureException(result);
            }

            return result;
        }

        private async Task ExecuteWithTracking<TResult>(
            string instanceId,
            Task<TResult> task,
            AggregationResult<TResult> result)
        {
            try
            {
                var taskResult = await task;
                result.AddSuccess(instanceId, taskResult);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Instance {InstanceId} failed during aggregation",
                    instanceId
                );

                var statusCode = ex is MoodleUpstreamException moodleEx
                    ? moodleEx.MoodleStatusCode
                    : null;

                result.AddFailure(instanceId, ex.Message, statusCode);
            }
        }

        private StandardException CreatePartialFailureException<TResult>(AggregationResult<TResult> result)
        {
            var exception = new StandardException(
                ErrorCode.PartialInstanceFailure,
                "Some instances failed to respond.",
                207 // Multi-Status
            );

            exception.Metadata = new Dictionary<string, object>
            {
                ["successCount"] = result.SuccessCount,
                ["failureCount"] = result.FailureCount,
                ["totalCount"] = result.TotalCount,
                ["failedInstances"] = result.Failures.ToList()
            };

            return exception;
        }
    }

    /// <summary>
    /// Result of multi-instance aggregation (thread-safe)
    /// </summary>
    public class AggregationResult<TResult>
    {
        private readonly ConcurrentBag<InstanceResult<TResult>> _successfulInstances = new();
        private readonly ConcurrentBag<InstanceFailureDetail> _failures = new();

        public IReadOnlyCollection<InstanceResult<TResult>> SuccessfulInstances => _successfulInstances;
        public IReadOnlyCollection<InstanceFailureDetail> Failures => _failures;

        public int SuccessCount => _successfulInstances.Count;
        public int FailureCount => _failures.Count;
        public int TotalCount => SuccessCount + FailureCount;

        public void AddSuccess(string instanceId, TResult result)
        {
            _successfulInstances.Add(new InstanceResult<TResult>
            {
                InstanceId = instanceId,
                Result = result
            });
        }

        public void AddFailure(string instanceId, string error, int? statusCode = null)
        {
            _failures.Add(new InstanceFailureDetail
            {
                InstanceId = instanceId,
                Error = error,
                StatusCode = statusCode
            });
        }
    }

    /// <summary>
    /// Successful result from a single instance
    /// </summary>
    public class InstanceResult<TResult>
    {
        public required string InstanceId { get; init; }
        public required TResult Result { get; init; }
    }
}
