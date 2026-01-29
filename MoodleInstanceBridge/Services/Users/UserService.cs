using Azure;
using LearningHub.Nhs.Models.Moodle;
using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Contracts.Aggregate;
using MoodleInstanceBridge.Contracts.Errors;
using MoodleInstanceBridge.Contracts.Payloads;
using MoodleInstanceBridge.Interfaces;
using MoodleInstanceBridge.Interfaces.Services;
using MoodleInstanceBridge.Models.Configuration;
using MoodleInstanceBridge.Models.Courses;
using MoodleInstanceBridge.Models.Users;
using MoodleInstanceBridge.Services.Orchestration;
using System.Security.Cryptography.X509Certificates;

namespace MoodleInstanceBridge.Services.Users
{
    /// <summary>
    /// Service for looking up Moodle user IDs across instances
    /// Uses orchestrator for multi-instance coordination
    /// </summary>
    public class UserService : IUserService
    {
        private readonly MultiInstanceOrchestrator<List<MoodleUser>> _allInstancesOrchestrator;
        private readonly TargetedInstanceOrchestrator _courseOrchestrator;
        private readonly TargetedInstanceOrchestrator _completionOrchestrator;
        private readonly TargetedInstanceOrchestrator _userDataOrchestrator;
        private readonly TargetedInstanceOrchestrator _recentCoursesOrchestrator;
        private readonly TargetedInstanceOrchestrator _certificatesOrchestrator;
        private readonly IMoodleIntegrationService _moodleIntegrationService;
        private readonly ILogger<UserService> _logger;

        public UserService(
           MultiInstanceOrchestrator<List<MoodleUser>> allInstancesOrchestrator,
           TargetedInstanceOrchestrator courseOrchestrator,
           TargetedInstanceOrchestrator completionOrchestrator,
           TargetedInstanceOrchestrator userDataOrchestrator,
           TargetedInstanceOrchestrator recentCoursesOrchestrator,
           TargetedInstanceOrchestrator certificatesOrchestrator,
           IMoodleIntegrationService moodleIntegrationService,
           ILogger<UserService> logger)
        {
            _allInstancesOrchestrator = allInstancesOrchestrator;
            _courseOrchestrator = courseOrchestrator;
            _completionOrchestrator = completionOrchestrator;
            _userDataOrchestrator = userDataOrchestrator;
            _recentCoursesOrchestrator = recentCoursesOrchestrator;
            _certificatesOrchestrator = certificatesOrchestrator;
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

            return await _allInstancesOrchestrator.ExecuteAcrossInstancesAsync(
                operationName: $"User lookup by email: {email}",
                instanceOperation: (config, ct) => GetUserFromInstanceAsync(config, email, ct),
                resultAggregator: AggregateUserResults,
                createEmptyResponse: () => new MoodleUserIdsResponse(),
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<AggregateResponse<UserCoursePayload>> GetUserCoursesAsync(
            UserIdsRequest userIdsRequest,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Starting user courses lookup for {Count} instance(s)",
                userIdsRequest.UserIds.Count
            );

            if (userIdsRequest == null)
                throw new ArgumentNullException(nameof(userIdsRequest));

            if (userIdsRequest.UserIds == null)
                throw new ArgumentNullException(nameof(userIdsRequest.UserIds));

            if (_courseOrchestrator == null)
                throw new InvalidOperationException("_courseOrchestrator is null");

            if (_moodleIntegrationService == null)
                throw new InvalidOperationException("_moodleIntegrationService is null");

            _logger.LogInformation(
                "Starting user courses lookup for {Count} instance(s)",
                userIdsRequest.UserIds.Count
            );

            var response = new AggregateResponse<UserCoursePayload>();

            Task<List<MoodleCourseResponseModel>> InstanceOperation(
                MoodleInstanceConfig config,
                int userId,
                CancellationToken ct)
            {
                if (config == null)
                    throw new ArgumentNullException(nameof(config));

                return _moodleIntegrationService.GetUserCoursesAsync(config, userId, ct);
            }

            var results = await _courseOrchestrator.ExecuteAcrossTargetedInstancesAsync(
                operationName: "User courses lookup",
                instanceUserIds: userIdsRequest.UserIds,
                instanceOperation: InstanceOperation,
                cancellationToken: cancellationToken
            );

            return AggregateResults(results, courses => new UserCoursePayload { Courses = courses });
        }

        /// <inheritdoc />
        public async Task<AggregateResponse<CourseCompletionStatusPayload>> GetCourseCompletionStatusAsync(
            UserIdsRequest userIdsRequest,
            int courseId,
            CancellationToken cancellationToken = default)
        {
            var response = new AggregateResponse<CourseCompletionStatusPayload>();

            var results = await _completionOrchestrator.ExecuteAcrossTargetedInstancesAsync(
                operationName: $"Course completion lookup for course {courseId}",
                instanceUserIds: userIdsRequest.UserIds,
                instanceOperation: (config, userId, ct) => _moodleIntegrationService.GetCourseCompletionStatusAsync(config, userId, courseId, ct),
                cancellationToken: cancellationToken
            );

            return AggregateResults(results, completions => new CourseCompletionStatusPayload { Completions = completions });
        }

        /// <inheritdoc />
        public async Task<AggregateResponse<UsersPayload>> GetUsersAsync(
           UserIdsRequest userIdsRequest,
           CancellationToken cancellationToken = default)
        {
            var response = new AggregateResponse<UsersPayload>();

            var results = await _userDataOrchestrator.ExecuteAcrossTargetedInstancesAsync(
                operationName: "User data lookup",
                instanceUserIds: userIdsRequest.UserIds,
                instanceOperation: (config, userId, ct) => _moodleIntegrationService.GetUsersAsync(config, userId, ct),
                cancellationToken: cancellationToken
            );

            return AggregateResults(results, users => new UsersPayload { Users = users });
        }

        ///// <inheritdoc />
        public async Task<AggregateResponse<RecentCoursesPayload>> GetRecentCoursesAsync(
            UserIdsRequest userIdsRequest,
            CancellationToken cancellationToken = default)
        {
            var response = new AggregateResponse<RecentCoursesPayload>();

            var results = await _recentCoursesOrchestrator.ExecuteAcrossTargetedInstancesAsync(
                operationName: "Recent courses lookup",
                instanceUserIds: userIdsRequest.UserIds,
                instanceOperation: (config, userId, ct) =>
                    _moodleIntegrationService.GetRecentCoursesAsync(config, userId, ct),
                cancellationToken: cancellationToken
            );

            return AggregateResults(results, courses => new RecentCoursesPayload { Courses = courses });

        }

        /// <inheritdoc />
        public async Task<AggregateResponse<UserCertificatesPayload>> GetUserCertificatesAsync(
            UserIdsRequest userIdsRequest,
            CancellationToken cancellationToken = default)
        {
            var response = new AggregateResponse<UserCertificatesPayload>();

            var results = await _certificatesOrchestrator.ExecuteAcrossTargetedInstancesAsync(
                operationName: "User certificates lookup",
                instanceUserIds: userIdsRequest.UserIds,
                instanceOperation: (config, userId, ct) => _moodleIntegrationService.GetUserCertificatesAsync(config, userId, ct),
                cancellationToken: cancellationToken
            );

            return AggregateResults(results, certificates => new UserCertificatesPayload { Certificates = certificates });
        }

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
            IEnumerable<(string InstanceName, List<MoodleUser>? Result, InstanceError? Error)> results)
        {
            foreach (var (instanceName, users, error) in results)
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
                        Instance = instanceName,
                        UserId = user
                    });
                }
            }
        }

        /// <summary>
        /// Generic helper to aggregate orchestrator results into an AggregateResponse
        /// </summary>
        /// <typeparam name="TPayload">The payload type for the response</typeparam>
        /// <typeparam name="TData">The data type returned from each instance</typeparam>
        /// <param name="results">Results from the orchestrator</param>
        /// <param name="payloadFactory">Function to create payload from data</param>
        /// <returns>Aggregated response with results from all instances</returns>
        private AggregateResponse<TPayload> AggregateResults<TPayload, TData>(
            IEnumerable<(string InstanceId, TData? Result, InstanceError? Error)> results,
            Func<TData, TPayload> payloadFactory)
            where TPayload : class
            where TData : class
        {
            var response = new AggregateResponse<TPayload>();

            foreach (var (instanceName, result, error) in results)
            {
                if (error != null)
                {
                    response.Results.Add(new AggregateResult<TPayload>
                    {
                        Instance = instanceName,
                        Data = null,
                        Error = error
                    });
                }
                else if (result != null)
                {
                    response.Results.Add(new AggregateResult<TPayload>
                    {
                        Instance = instanceName,
                        Data = payloadFactory(result),
                        Error = null
                    });
                }
            }

            return response;
        }
    }
}
