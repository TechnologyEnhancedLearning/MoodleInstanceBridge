using LearningHub.Nhs.Models.Moodle;
using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Interfaces;
using MoodleInstanceBridge.Interfaces.Services;
using MoodleInstanceBridge.Models.Configuration;
using MoodleInstanceBridge.Services.WebServiceClient;

namespace MoodleInstanceBridge.Services.Moodle
{
    /// <summary>
    /// Service for Moodle course-related operations
    /// </summary>
    public class MoodleCourseService : IMoodleCourseService
    {
        private readonly IMoodleWebServiceClient _webServiceClient;
        private readonly ILogger<MoodleCourseService> _logger;

        public MoodleCourseService(IMoodleWebServiceClient webServiceClient, ILogger<MoodleCourseService> logger)
        {
            _webServiceClient = webServiceClient;
            _logger = logger;
        }

        public async Task<List<MoodleCategory>> GetCategoriesAsync(
            MoodleInstanceConfig config,
            CancellationToken cancellationToken = default)
        {
            var url = MoodleUrlBuilder.BuildCategoriesUrl(config);
            var categories = await _webServiceClient.ExecuteRequestAsync<List<MoodleCategory>>(
                config,
                url,
                "core_course_get_categories",
                cancellationToken);

            _logger.LogInformation(
                "Found {Count} categories in instance {Instance}",
                categories?.Count ?? 0,
                config.ShortName
            );

            return categories ?? new List<MoodleCategory>();
        }

        public async Task<MoodleCoursesResponseModel> GetCoursesByFieldAsync(
            MoodleInstanceConfig config,
            string field,
            string value,
            CancellationToken cancellationToken = default)
        {
            ValidateInputs(field, value);

            var url = MoodleUrlBuilder.BuildCoursesByFieldUrl(config, field, value);
            var response = await _webServiceClient.ExecuteRequestAsync<MoodleCoursesResponseModel>(
                config,
                url,
                "core_course_get_courses_by_field",
                cancellationToken);          

            return response ?? new MoodleCoursesResponseModel();
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
