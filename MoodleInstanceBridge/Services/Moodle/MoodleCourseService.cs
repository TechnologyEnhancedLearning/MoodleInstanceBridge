using LearningHub.Nhs.Models.Moodle;
using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Helpers;
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

            foreach (var item in response.Courses)
            {
                item.CourseUrl = CourseHelper.GetCourseUrl(config.BaseUrl,item.Id);
            }

            return response ?? new MoodleCoursesResponseModel();
        }

        public async Task<List<MoodleSubCategoryResponseModel>> GetSubCategoriesAsync(
            MoodleInstanceConfig config,
            int categoryId,
            CancellationToken cancellationToken = default)
        {
            var url = MoodleUrlBuilder.BuildSubCategoriesUrl(config, categoryId);
            var categories = await _webServiceClient.ExecuteRequestAsync<List<MoodleSubCategoryResponseModel>>(
                config,
                url,
                "core_course_get_categories",
                cancellationToken);

            if (categories == null || categories.Count == 0)
                return new List<MoodleSubCategoryResponseModel>();

            var subcategories = categories
                .Where(sc => sc.Id != categoryId && sc.Parent == categoryId)
                .ToList();

            _logger.LogInformation(
                "Found {Count} subcategories for category {CategoryId} in instance {Instance}",
                subcategories.Count,
                categoryId,
                config.ShortName
            );

            return subcategories;
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
