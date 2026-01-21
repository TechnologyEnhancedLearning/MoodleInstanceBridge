using LearningHub.Nhs.Models.Moodle;
using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Interfaces;
using MoodleInstanceBridge.Interfaces.Services;
using MoodleInstanceBridge.Models.Configuration;
using MoodleInstanceBridge.Models.Errors;
using MoodleInstanceBridge.Models.Users;
using MoodleInstanceBridge.Services.Orchestration;

namespace MoodleInstanceBridge.Services.Users
{
    /// <summary>
    /// Service for looking up Moodle user IDs across instances
    /// Uses orchestrator for multi-instance coordination
    /// </summary>
    public class UserService : IUserService
    {
        private readonly MultiInstanceOrchestrator<List<MoodleUser>> _orchestrator;
        private readonly MultiInstanceOrchestrator<MoodleCourseResponseModel> _courseOrchestrator;
        private readonly IMoodleIntegrationService _moodleIntegrationService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            MultiInstanceOrchestrator<List<MoodleUser>> orchestrator,
            MultiInstanceOrchestrator<MoodleCourseResponseModel> courseOrchestrator,
            IMoodleIntegrationService moodleIntegrationService,
            ILogger<UserService> logger)
        {
            _orchestrator = orchestrator;
            _moodleIntegrationService = moodleIntegrationService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<MoodleUserIdsResponse> GetMoodleUserIdsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));

            return await _orchestrator.ExecuteAcrossInstancesAsync(
                operationName: $"User lookup by email: {email}",
                instanceOperation: (config, ct) => GetUserFromInstanceAsync(config, email, ct),
                resultAggregator: AggregateUserResults,
                createEmptyResponse: () => new MoodleUserIdsResponse(),
                cancellationToken: cancellationToken
            );
        }

        // <inheritdoc />
        //public async Task<UserCoursesResponse> GetUserCoursesAsync(
        //    int userId,
        //    CancellationToken cancellationToken = default)
        //{
        //    return await _courseOrchestrator.ExecuteAcrossInstancesAsync(
        //        operationName: $"User courses lookup for user ID: {userId}",
        //        instanceOperation: (config, ct) => GetUserCoursesFromInstanceAsync(config, userId, ct),
        //        resultAggregator: AggregateUserCoursesResults,
        //        createEmptyResponse: () => new UserCoursesResponse(),
        //        cancellationToken: cancellationToken
        //    );
        //}


        /// <summary>
        /// Get user from a specific Moodle instance - domain logic only
        /// </summary>
        private async Task<(string ShortName, List<MoodleUser>? Result, InstanceError? Error)> GetUserFromInstanceAsync(
            MoodleInstanceConfig config,
            string email,
            CancellationToken cancellationToken)
        {
            try
            {
                // Call Moodle Web Service to get users by email
                var users = await _moodleIntegrationService.GetUsersByFieldAsync(
                    config,
                    "email",
                    email,
                    cancellationToken
                );

                if (users.Any())
                {
                    _logger.LogInformation(
                        "Found {Count} user(s) in instance {Instance}",
                        users.Count,
                        config.ShortName
                    );
                }
                else
                {
                    _logger.LogInformation(
                        "User not found in instance {Instance}",
                        config.ShortName
                    );
                }

                return (config.ShortName, users, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error looking up user in instance {Instance}",
                    config.ShortName
                );

                return (config.ShortName, null, InstanceErrorHelper.CreateFromException(config.ShortName, ex));
            }
        }

        /// <summary>
        /// Aggregate user lookup results from multiple instances
        /// </summary>
        private void AggregateUserResults(
            MoodleUserIdsResponse response,
            IEnumerable<(string ShortName, List<MoodleUser>? Result, InstanceError? Error)> results)
        {
            foreach (var (shortName, users, error) in results)
            {
                if (error != null)
                {
                    response.Errors.Add(error);
                }
                else if (users != null && users.Any())
                {
                    var user = users.First(); // Should only be one user per email
                    response.MoodleUserIds.Add(new MoodleUserIdResult
                    {
                        Instance = shortName,
                        UserId = user
                    });
                }
            }
        }
    }
}
