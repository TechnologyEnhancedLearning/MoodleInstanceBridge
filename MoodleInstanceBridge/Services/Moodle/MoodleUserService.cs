using LearningHub.Nhs.Models.Moodle;
using MoodleInstanceBridge.Interfaces;
using MoodleInstanceBridge.Interfaces.Services;
using MoodleInstanceBridge.Models.Configuration;
using MoodleInstanceBridge.Services.WebServiceClient;

namespace MoodleInstanceBridge.Services.Moodle
{
    /// <summary>
    /// Service for Moodle user-related operations
    /// </summary>
    public class MoodleUserService : IMoodleUserService
    {
        private readonly IMoodleWebServiceClient _webServiceClient;
        private readonly ILogger<MoodleUserService> _logger;

        public MoodleUserService(IMoodleWebServiceClient webServiceClient, ILogger<MoodleUserService> logger)
        {
            _webServiceClient = webServiceClient;
            _logger = logger;
        }

        public async Task<List<MoodleUser>> GetUsersByFieldAsync(
            MoodleInstanceConfig config,
            string field,
            string value,
            CancellationToken cancellationToken = default)
        {
            ValidateInputs(field, value);

            var url = MoodleUrlBuilder.BuildUsersByFieldUrl(config, field, value);
            var users = await _webServiceClient.ExecuteRequestAsync<List<MoodleUser>>(
                config, 
                url, 
                "core_user_get_users_by_field", 
                cancellationToken);

            _logger.LogInformation(
                "Found {Count} users for {Field}={Value} in instance {Instance}",
                users?.Count ?? 0,
                field,
                value,
                config.ShortName
            );

            return users ?? new List<MoodleUser>();
        }

        private static void ValidateInputs(string field, string value)
        {
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentException("Field cannot be null or whitespace.", nameof(field));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
        }
    }
}
