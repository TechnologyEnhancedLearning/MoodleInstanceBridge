using MoodleInstanceBridge.Interfaces;
using MoodleInstanceBridge.Models.Configuration;
using MoodleInstanceBridge.Models.Errors;

namespace MoodleInstanceBridge.Services.Orchestration
{
    /// <summary>
    /// Generic orchestrator for executing operations across multiple Moodle instances in parallel
    /// Handles: configuration fetching, fan-out, parallel execution, result aggregation, and error handling
    /// </summary>
    /// <typeparam name="TResult">The type of result returned from each instance</typeparam>
    public class MultiInstanceOrchestrator<TResult>
    {
        private readonly IInstanceConfigurationService _configService;
        private readonly ILogger<MultiInstanceOrchestrator<TResult>> _logger;

        public MultiInstanceOrchestrator(
            IInstanceConfigurationService configService,
            ILogger<MultiInstanceOrchestrator<TResult>> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        /// <summary>
        /// Execute an operation across all enabled Moodle instances in parallel
        /// </summary>
        /// <typeparam name="TResponse">The type of aggregated response</typeparam>
        /// <param name="operationName">Name of the operation for logging</param>
        /// <param name="instanceOperation">Function to execute on each instance</param>
        /// <param name="resultAggregator">Function to aggregate results into response</param>
        /// <param name="createEmptyResponse">Function to create empty response object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Aggregated response from all instances</returns>
        public async Task<TResponse> ExecuteAcrossInstancesAsync<TResponse>(
            string operationName,
            Func<MoodleInstanceConfig, CancellationToken, Task<(string ShortName, TResult? Result, InstanceError? Error)>> instanceOperation,
            Action<TResponse, IEnumerable<(string ShortName, TResult? Result, InstanceError? Error)>> resultAggregator,
            Func<TResponse> createEmptyResponse,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting {OperationName} across all Moodle instances", operationName);

            var response = createEmptyResponse();

            // Get all enabled instance configurations
            var configurations = await _configService.GetAllConfigurationsAsync(cancellationToken);

            if (!configurations.Any())
            {
                _logger.LogWarning("No enabled Moodle instances configured for {OperationName}", operationName);
                return response;
            }

            _logger.LogInformation(
                "Querying {Count} Moodle instances for {OperationName}",
                configurations.Count,
                operationName
            );

            // Create fan-out tasks for parallel execution
            var lookupTasks = configurations
                .Select(config => instanceOperation(config, cancellationToken))
                .ToList();

            // Wait for all tasks to complete (WhenAll allows all to finish even if some fail)
            var results = await Task.WhenAll(lookupTasks);

            // Aggregate results using provided aggregator
            resultAggregator(response, results);

            var successCount = results.Count(r => r.Error == null);
            var errorCount = results.Count(r => r.Error != null);

            _logger.LogInformation(
                "{OperationName} completed: {SuccessCount} successful, {ErrorCount} errors",
                operationName,
                successCount,
                errorCount
            );

            return response;
        }

        /// <summary>
        /// Execute an operation on a single instance with error handling
        /// </summary>
        /// <param name="shortName">Instance short name</param>
        /// <param name="operation">Operation to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple with short name, result, and error</returns>
        public async Task<(string ShortName, TResult? Result, InstanceError? Error)> ExecuteOnInstanceAsync(
            string shortName,
            Func<MoodleInstanceConfig, CancellationToken, Task<TResult>> operation,
            CancellationToken cancellationToken)
        {
            try
            {
                // Get the configuration for this instance
                var config = await _configService.GetConfigurationAsync(shortName, cancellationToken);
                if (config == null)
                {
                    _logger.LogError("Configuration not found for instance {Instance}", shortName);
                    return (shortName, default, InstanceErrorHelper.CreateConfigurationError(shortName));
                }

                // Execute operation
                var result = await operation(config, cancellationToken);
                return (shortName, result, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error executing operation on instance {Instance}",
                    shortName
                );

                return (shortName, default, InstanceErrorHelper.CreateFromException(shortName, ex));
            }
        }
    }
}
