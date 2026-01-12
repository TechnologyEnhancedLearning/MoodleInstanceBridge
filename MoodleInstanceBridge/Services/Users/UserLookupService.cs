using MoodleInstanceBridge.Models.Users;
using MoodleInstanceBridge.Services.Configuration;
using MoodleInstanceBridge.Services.Moodle;
using System;
using System.Collections.Concurrent;

namespace MoodleInstanceBridge.Services.Users
{
    /// <summary>
    /// Service for looking up Moodle user IDs across instances with parallel fan-out
    /// </summary>
    public class UserLookupService : IUserLookupService
    {
        private readonly IInstanceConfigurationService _configService;
        private readonly IMoodleClient _moodleClient;
        private readonly ILogger<UserLookupService> _logger;

        public UserLookupService(
            IInstanceConfigurationService configService,
            IMoodleClient moodleClient,
            ILogger<UserLookupService> logger)
        {
            _configService = configService;
            _moodleClient = moodleClient;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<MoodleUserIdsResponse> GetMoodleUserIdsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));

            _logger.LogInformation("Looking up Moodle user IDs for email: {Email}", email);

            var response = new MoodleUserIdsResponse();

            // Get all enabled instance configurations
            var configurations = await _configService.GetAllConfigurationsAsync(cancellationToken);

            if (!configurations.Any())
            {
                _logger.LogWarning("No enabled Moodle instances configured");
                return response;
            }

            _logger.LogInformation(
                "Querying {Count} Moodle instances for user with email {Email}",
                configurations.Count,
                email
            );

            //  https://hee-tis.atlassian.net/browse/TD-6563 Create fan-out tasks for parallel execution
            // creates and starts multiple asynchronous lookup operations in parallel
            var lookupTasks = configurations
                .Select(config => LookupUserInInstanceAsync(config.ShortName, email, cancellationToken))
                .ToList();

            // Wait for all tasks to complete (using WhenAll to allow all to finish even if some fail)
            var results = await Task.WhenAll(lookupTasks);

            // Process results
            foreach (var (shortName, userId, error) in results)
            {
                if (error != null)
                {
                    response.Errors.Add(error);
                }
                else
                {
                    response.MoodleUserIds.Add(new MoodleUserIdResult
                    {
                        Instance = shortName,
                        UserId = userId
                    });
                }
            }

            _logger.LogInformation(
                "User lookup completed: {SuccessCount} successful, {ErrorCount} errors",
                response.MoodleUserIds.Count,
                response.Errors.Count
            );

            return response;
        }

        /// <summary>
        /// Look up a user in a specific Moodle instance
        /// </summary>
        private async Task<(string ShortName, int? UserId, InstanceError? Error)> LookupUserInInstanceAsync(
            string shortName,
            string email,
            CancellationToken cancellationToken)
        {
            try
            {
                // Get the configuration for this instance
                var config = await _configService.GetConfigurationAsync(shortName, cancellationToken);
                if (config == null)
                {
                    _logger.LogError("Configuration not found for instance {Instance}", shortName);
                    return (shortName, null, new InstanceError
                    {
                        Instance = shortName,
                        Code = "CONFIGURATION_ERROR",
                        Message = "Instance configuration not found"
                    });
                }

                // Call Moodle Web Service to get users by email
                var users = await _moodleClient.GetUsersByFieldAsync(
                    config,
                    "email",
                    email,
                    cancellationToken
                );

                // If user found, return the ID, otherwise return null
                if (users.Any())
                {
                    var user = users.First(); // Should only be one user per email
                    _logger.LogInformation(
                        "Found user {UserId} in instance {Instance}",
                        user.Id,
                        shortName
                    );
                    return (shortName, user.Id, null);
                }
                else
                {
                    _logger.LogInformation(
                        "User not found in instance {Instance}",
                        shortName
                    );
                    return (shortName, null, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error looking up user in instance {Instance}",
                    shortName
                );

                // Determine appropriate error code
                string errorCode = "INSTANCE_UNAVAILABLE";
                string errorMessage = "Moodle instance could not be reached";

                if (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
                {
                    errorCode = "TIMEOUT";
                    errorMessage = "Request to Moodle instance timed out";
                }
                else if (ex.Message.Contains("connect", StringComparison.OrdinalIgnoreCase))
                {
                    errorCode = "CONNECTION_ERROR";
                    errorMessage = "Failed to connect to Moodle instance";
                }

                // ❗ LookupUserInInstanceAsync must never throw It should always return a tuple, even on failure.
                return (shortName, null, new InstanceError
                {
                    Instance = shortName,
                    Code = errorCode,
                    Message = errorMessage
                });
            }
        }
    }
}
