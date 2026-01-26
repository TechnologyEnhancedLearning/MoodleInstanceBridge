using MoodleInstanceBridge.Contracts.Errors;
using MoodleInstanceBridge.Interfaces;
using MoodleInstanceBridge.Models.Configuration;

namespace MoodleInstanceBridge.Services.Orchestration
{
    /// <summary>
    /// Orchestrator for executing operations across specific Moodle instances (not all instances)
    /// Handles: instance lookup, parallel execution, result aggregation, and error handling
    /// Use this when you know exactly which instances to query (e.g., from a userIds map)
    /// </summary>
    /// <typeparam name="TResult">The type of result returned from each instance</typeparam>
    public class TargetedInstanceOrchestrator
    {
        private readonly IInstanceConfigurationService _configService;
        private readonly ILogger<TargetedInstanceOrchestrator> _logger;

        public TargetedInstanceOrchestrator(
            IInstanceConfigurationService configService,
            ILogger<TargetedInstanceOrchestrator> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        /// <summary>
        /// Execute an operation across specific Moodle instances in parallel
        /// </summary>
        /// <typeparam name="TResult">The result type returned per instance</typeparam>
        /// <param name="operationName">Name of the operation for logging</param>
        /// <param name="instanceUserIds">Map of instance IDs to user IDs</param>
        /// <param name="instanceOperation">Function to execute on each instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Results from all instances with errors</returns>
        public async Task<IEnumerable<(string InstanceId, TResult? Result, InstanceError? Error)>>
            ExecuteAcrossTargetedInstancesAsync<TResult>(
                string operationName,
                Dictionary<string, int> instanceUserIds,
                Func<MoodleInstanceConfig, int, CancellationToken, Task<TResult>> instanceOperation,
                CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Starting {OperationName} across {Count} targeted instance(s)",
                operationName,
                instanceUserIds.Count
            );

            var tasks = instanceUserIds
                .Select(kvp => ExecuteForInstanceAsync(
                    kvp.Key,
                    kvp.Value,
                    instanceOperation,
                    cancellationToken))
                .ToList();

            var results = await Task.WhenAll(tasks);

            var successCount = results.Count(r => r.Error == null);
            _logger.LogInformation(
                "{OperationName} completed: {SuccessCount} successful, {ErrorCount} errors",
                operationName,
                successCount,
                results.Length - successCount
            );

            return results;
        }

        /// <summary>
        /// Execute operation for a specific instance with consistent error handling and logging
        /// </summary>
        private async Task<(string InstanceId, TResult? Result, InstanceError? Error)>
            ExecuteForInstanceAsync<TResult>(
                string instanceId,
                int userId,
                Func<MoodleInstanceConfig, int, CancellationToken, Task<TResult>> operation,
                CancellationToken cancellationToken)
        {
            try
            {
                var config = await GetConfigByShortNameAsync(instanceId, cancellationToken);
                if (config == null)
                {
                    return (instanceId, default, new InstanceError
                    {
                        Instance = instanceId,
                        Code = "InstanceNotFound",
                        Message = $"Instance '{instanceId}' not found or not enabled"
                    });
                }

                var result = await operation(config, userId, cancellationToken);
                return (instanceId, result, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error executing operation for user {UserId} in instance {Instance}",
                    userId,
                    instanceId);

                return (instanceId, default, InstanceErrorHelper.CreateFromException(instanceId, ex));
            }
        }

        private async Task<MoodleInstanceConfig?> GetConfigByShortNameAsync(
            string shortName,
            CancellationToken cancellationToken)
        {
            var configs = await _configService.GetAllConfigurationsAsync(cancellationToken);
            return configs.FirstOrDefault(c =>
                c.ShortName.Equals(shortName, StringComparison.OrdinalIgnoreCase));
        }
    }

}
