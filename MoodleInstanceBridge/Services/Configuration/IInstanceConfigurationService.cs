using MoodleInstanceBridge.Models.Configuration;

namespace MoodleInstanceBridge.Services.Configuration
{
    /// <summary>
    /// Service for loading and managing Moodle instance configurations
    /// </summary>
    public interface IInstanceConfigurationService
    {
        /// <summary>
        /// Get all active Moodle instance configurations with secrets loaded from Key Vault
        /// </summary>
        /// <returns>List of active instance configurations</returns>
        Task<IReadOnlyList<MoodleInstanceConfig>> GetAllConfigurationsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a specific instance configuration by short name
        /// </summary>
        /// <param name="shortName">Instance short name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instance configuration or null if not found</returns>
        Task<MoodleInstanceConfig?> GetConfigurationAsync(string shortName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refresh configurations from database and Key Vault
        /// </summary>
        /// <returns>Validation results for all configurations</returns>
        Task<IReadOnlyList<ConfigurationValidationResult>> RefreshConfigurationsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Validate a specific configuration
        /// </summary>
        /// <param name="config">Configuration to validate</param>
        /// <returns>Validation result</returns>
        ConfigurationValidationResult ValidateConfiguration(MoodleInstanceConfig config);

        /// <summary>
        /// Get the last refresh time
        /// </summary>
        DateTime? LastRefreshTime { get; }

        /// <summary>
        /// Get validation errors from the last refresh
        /// </summary>
        IReadOnlyList<ConfigurationValidationResult> LastValidationResults { get; }
    }
}
