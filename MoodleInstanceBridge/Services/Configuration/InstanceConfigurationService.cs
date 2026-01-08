using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using MoodleInstanceBridge.Data;
using MoodleInstanceBridge.Models.Configuration;
using System.Collections.Concurrent;

namespace MoodleInstanceBridge.Services.Configuration
{
    /// <summary>
    /// Service for loading and managing Moodle instance configurations
    /// Thread-safe implementation with caching
    /// </summary>
    public class InstanceConfigurationService : IInstanceConfigurationService, IDisposable
    {
        private readonly IDbContextFactory<MoodleInstanceContext> _contextFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InstanceConfigurationService> _logger;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private readonly SecretClient? _secretClient;
        
        private ConcurrentDictionary<string, MoodleInstanceConfig> _cachedConfigurations = new();
        private List<ConfigurationValidationResult> _lastValidationResults = new();
        private DateTime? _lastRefreshTime;
        private bool _disposed = false;

        public DateTime? LastRefreshTime => _lastRefreshTime;
        public IReadOnlyList<ConfigurationValidationResult> LastValidationResults => _lastValidationResults.AsReadOnly();

        public InstanceConfigurationService(
            IDbContextFactory<MoodleInstanceContext> contextFactory,
            IConfiguration configuration,
            ILogger<InstanceConfigurationService> logger)
        {
            _contextFactory = contextFactory;
            _configuration = configuration;
            _logger = logger;

            // Initialize Azure Key Vault client if configured
            var keyVaultUrl = _configuration["KeyVault:VaultUrl"];
            if (!string.IsNullOrEmpty(keyVaultUrl))
            {
                try
                {
                    _secretClient = new SecretClient(
                        new Uri(keyVaultUrl),
                        new DefaultAzureCredential()
                    );
                    _logger.LogInformation("Azure Key Vault client initialized for {KeyVaultUrl}", keyVaultUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize Azure Key Vault client for {KeyVaultUrl}", keyVaultUrl);
                }
            }
            else
            {
                _logger.LogWarning("KeyVault:VaultUrl not configured. Secrets will not be loaded from Key Vault.");
            }
        }

        public async Task<IReadOnlyList<MoodleInstanceConfig>> GetAllConfigurationsAsync(CancellationToken cancellationToken = default)
        {
            // Return cached configurations if available
            if (_cachedConfigurations.IsEmpty)
            {
                await RefreshConfigurationsAsync(cancellationToken);
            }

            return _cachedConfigurations.Values
                .Where(c => c.IsEnabled)
                .ToList()
                .AsReadOnly();
        }

        public async Task<MoodleInstanceConfig?> GetConfigurationAsync(string shortName, CancellationToken cancellationToken = default)
        {
            // Ensure configurations are loaded
            if (_cachedConfigurations.IsEmpty)
            {
                await RefreshConfigurationsAsync(cancellationToken);
            }

            _cachedConfigurations.TryGetValue(shortName, out var config);
            return config;
        }

        public async Task<IReadOnlyList<ConfigurationValidationResult>> RefreshConfigurationsAsync(CancellationToken cancellationToken = default)
        {
            // Thread-safe refresh using semaphore
            await _refreshLock.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Starting configuration refresh");

                var newConfigurations = new ConcurrentDictionary<string, MoodleInstanceConfig>();
                var validationResults = new List<ConfigurationValidationResult>();

                // Load configurations from database
                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
                var dbConfigs = await context.MoodleInstanceConfigs
                    .Where(c => c.IsEnabled)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Loaded {Count} instance configurations from database", dbConfigs.Count);

                // Process each configuration
                foreach (var dbConfig in dbConfigs)
                {
                    try
                    {
                        // Load secret from Key Vault
                        var apiToken = await GetSecretAsync(dbConfig.TokenSecretName, cancellationToken);

                        if (string.IsNullOrEmpty(apiToken))
                        {
                            var validationResult = new ConfigurationValidationResult(false)
                            {
                                InstanceShortName = dbConfig.ShortName
                            };
                            validationResult.AddError($"Failed to load API token from Key Vault secret '{dbConfig.TokenSecretName}'");
                            validationResults.Add(validationResult);

                            _logger.LogError(
                                "Failed to load secret for instance {InstanceShortName}: {SecretName}",
                                dbConfig.ShortName,
                                dbConfig.TokenSecretName
                            );
                            continue;
                        }

                        // Create MoodleInstanceConfig
                        var config = new MoodleInstanceConfig
                        {
                            ShortName = dbConfig.ShortName,
                            BaseUrl = dbConfig.BaseUrl,
                            ApiToken = apiToken,
                            EnabledEndpoints = string.IsNullOrEmpty(dbConfig.EnabledEndpoints)
                                ? Array.Empty<string>()
                                : dbConfig.EnabledEndpoints.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(e => e.Trim())
                                    .ToArray(),
                            Weighting = dbConfig.Weighting,
                            IsEnabled = dbConfig.IsEnabled
                        };

                        // Validate configuration
                        var validation = ValidateConfiguration(config);
                        validationResults.Add(validation);

                        if (validation.IsValid)
                        {
                            newConfigurations[config.ShortName] = config;
                            _logger.LogInformation(
                                "Successfully loaded and validated configuration for instance {InstanceShortName}",
                                config.ShortName
                            );
                        }
                        else
                        {
                            _logger.LogError(
                                "Validation failed for instance {InstanceShortName}: {Errors}",
                                config.ShortName,
                                string.Join(", ", validation.Errors)
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        var validationResult = new ConfigurationValidationResult(false)
                        {
                            InstanceShortName = dbConfig.ShortName
                        };
                        validationResult.AddError($"Exception during configuration load: {ex.Message}");
                        validationResults.Add(validationResult);

                        _logger.LogError(
                            ex,
                            "Exception while processing configuration for instance {InstanceShortName}",
                            dbConfig.ShortName
                        );
                    }
                }

                // Update cache and metadata
                _cachedConfigurations = newConfigurations;
                _lastValidationResults = validationResults;
                _lastRefreshTime = DateTime.UtcNow;

                _logger.LogInformation(
                    "Configuration refresh completed: {SuccessCount} valid, {ErrorCount} invalid",
                    validationResults.Count(v => v.IsValid),
                    validationResults.Count(v => !v.IsValid)
                );

                return validationResults.AsReadOnly();
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        public ConfigurationValidationResult ValidateConfiguration(MoodleInstanceConfig config)
        {
            var result = new ConfigurationValidationResult(true)
            {
                InstanceShortName = config.ShortName
            };

            // Validate short name
            if (string.IsNullOrWhiteSpace(config.ShortName))
            {
                result.AddError("ShortName is required");
            }

            // Validate base URL
            if (string.IsNullOrWhiteSpace(config.BaseUrl))
            {
                result.AddError("BaseUrl is required");
            }
            else if (!Uri.TryCreate(config.BaseUrl, UriKind.Absolute, out var uri) ||
                     (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                result.AddError("BaseUrl must be a valid HTTP or HTTPS URL");
            }

            // Validate API token
            if (string.IsNullOrWhiteSpace(config.ApiToken))
            {
                result.AddError("ApiToken is required");
            }

            // Validate weighting
            if (config.Weighting < 0 || config.Weighting > 100)
            {
                result.AddError("Weighting must be between 0 and 100");
            }

            return result;
        }

        private async Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken)
        {
            if (_secretClient == null)
            {
                _logger.LogWarning("Key Vault client not initialized. Cannot retrieve secret {SecretName}", secretName);
                return null;
            }

            try
            {
                var secret = await _secretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);
                
                if (secret?.Value?.Value == null)
                {
                    _logger.LogError("Secret {SecretName} exists but has no value", secretName);
                    return null;
                }
                
                return secret.Value.Value;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogError("Secret {SecretName} not found in Key Vault", secretName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve secret {SecretName} from Key Vault", secretName);
                return null;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _refreshLock?.Dispose();
            _disposed = true;
        }
    }
}
