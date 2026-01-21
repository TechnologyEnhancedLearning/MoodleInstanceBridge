using LearningHub.Nhs.Models.Moodle;
using LearningHub.Nhs.Models.Moodle.API;
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

        public async Task<MoodleCourseResponseModel> GetUserCoursesAsync(
            MoodleInstanceConfig config,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

            var url = MoodleUrlBuilder.BuildUserCoursesUrl(config, userId);
            var courses = await _webServiceClient.ExecuteRequestAsync<MoodleCourseResponseModel>(
                config,
                url,
                "core_enrol_get_users_courses",
                cancellationToken);

            return courses ?? new MoodleCourseResponseModel();
        }

        public async Task<MoodleCourseCompletionModel> GetCourseCompletionStatusAsync(
            MoodleInstanceConfig config,
            int userId,
            int courseId,
            CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));
            if (courseId <= 0)
                throw new ArgumentException("Course ID must be greater than zero.", nameof(courseId));

            var url = MoodleUrlBuilder.BuildCourseCompletionStatusUrl(config, userId, courseId);
            var completion = await _webServiceClient.ExecuteRequestAsync<MoodleCourseCompletionModel>(
                config,
                url,
                "core_completion_get_course_completion_status",
                cancellationToken);

            _logger.LogInformation(
                "Retrieved course completion status for user {UserId}, course {CourseId} in instance {Instance}",
                userId,
                courseId,
                config.ShortName
            );

            return completion ?? new MoodleCourseCompletionModel();
        }

        public async Task<MoodleUserResponseModel> GetUsersAsync(
            MoodleInstanceConfig config,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

            var url = MoodleUrlBuilder.BuildGetUsersUrl(config, userId);
            var userResponse = await _webServiceClient.ExecuteRequestAsync<MoodleUserResponseModel>(
                config,
                url,
                "core_user_get_users",
                cancellationToken);

            _logger.LogInformation(
                "Retrieved user data for user {UserId} in instance {Instance}",
                userId,
                config.ShortName
            );

            return userResponse ?? new MoodleUserResponseModel();
        }

        public async Task<MoodleEnrolledCourseResponseModel> GetRecentCoursesAsync(
            MoodleInstanceConfig config,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

            // Try mylearningservice_get_recent_courses first, fallback to core_course_get_recent_courses
            var url = MoodleUrlBuilder.BuildRecentCoursesUrl(config, userId);
            var recentCourses = await _webServiceClient.ExecuteRequestAsync<MoodleEnrolledCourseResponseModel>(
                config,
                url,
                "mylearningservice_get_recent_courses",
                cancellationToken);

            return recentCourses ?? new MoodleEnrolledCourseResponseModel();
        }

        public async Task<MoodleUserCertificateResponseModel> GetUserCertificatesAsync(
            MoodleInstanceConfig config,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

            var url = MoodleUrlBuilder.BuildUserCertificatesUrl(config, userId);
            var certificates = await _webServiceClient.ExecuteRequestAsync<MoodleUserCertificateResponseModel>(
                config,
                url,
                "mylearningservice_get_user_certificates",
                cancellationToken);            

            return certificates ?? new MoodleUserCertificateResponseModel();
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
