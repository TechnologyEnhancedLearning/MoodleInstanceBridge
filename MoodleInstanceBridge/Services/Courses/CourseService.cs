using LearningHub.Nhs.Models.Moodle;
using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Contracts.Errors;
using MoodleInstanceBridge.Interfaces;
using MoodleInstanceBridge.Interfaces.Services;
using MoodleInstanceBridge.Models.Configuration;
using MoodleInstanceBridge.Models.Courses;
using MoodleInstanceBridge.Services.Orchestration;

namespace MoodleInstanceBridge.Services.Courses
{
    /// <summary>
    /// Service for looking up course data across instances
    /// Uses orchestrators for multi-instance coordination
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly MultiInstanceOrchestrator<List<MoodleCategory>> _categoryOrchestrator;
        private readonly MultiInstanceOrchestrator<MoodleCoursesResponseModel> _courseOrchestrator;
        private readonly IMoodleIntegrationService _moodleIntegrationService;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            MultiInstanceOrchestrator<List<MoodleCategory>> categoryOrchestrator,
            MultiInstanceOrchestrator<MoodleCoursesResponseModel> courseOrchestrator,
            IMoodleIntegrationService moodleIntegrationService,
            ILogger<CourseService> logger)
        {
            _categoryOrchestrator = categoryOrchestrator;
            _courseOrchestrator = courseOrchestrator;
            _moodleIntegrationService = moodleIntegrationService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<CategoriesResponse> GetCategoriesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _categoryOrchestrator.ExecuteAcrossInstancesAsync(
                operationName: "Category lookup",
                instanceOperation: (config, ct) => GetCategoriesFromInstanceAsync(config, ct),
                resultAggregator: AggregateCategoryResults,
                createEmptyResponse: () => new CategoriesResponse(),
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<CoursesResponse> SearchCoursesAsync(
            string field,
            string value,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentException("Field cannot be null or whitespace.", nameof(field));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));

            return await _courseOrchestrator.ExecuteAcrossInstancesAsync(
                operationName: $"Course search by {field}={value}",
                instanceOperation: (config, ct) => SearchCoursesInInstanceAsync(config, field, value, ct),
                resultAggregator: AggregateCourseResults,
                createEmptyResponse: () => new CoursesResponse(),
                cancellationToken: cancellationToken
            );
        }

        /// <summary>
        /// Get categories from a specific Moodle instance - domain logic only
        /// </summary>
        private async Task<(string ShortName, List<MoodleCategory>? Result, InstanceError? Error)> 
            GetCategoriesFromInstanceAsync(
                MoodleInstanceConfig config,
                CancellationToken cancellationToken)
        {
            try
            {
                // Call Moodle Web Service to get categories
                var categories = await _moodleIntegrationService.GetCategoriesAsync(config, cancellationToken);

                _logger.LogInformation(
                    "Found {Count} categories in instance {Instance}",
                    categories.Count,
                    config.ShortName
                );

                return (config.ShortName, categories, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting categories from instance {Instance}",
                    config.ShortName
                );

                return (config.ShortName, null, InstanceErrorHelper.CreateFromException(config.ShortName, ex));
            }
        }

        /// <summary>
        /// Search for courses in a specific Moodle instance - domain logic only
        /// </summary>
        private async Task<(string ShortName, MoodleCoursesResponseModel? Result, InstanceError? Error)> 
            SearchCoursesInInstanceAsync(
                MoodleInstanceConfig config,
                string field,
                string value,
                CancellationToken cancellationToken)
        {
            try
            {
                // Call Moodle Web Service to search for courses
                var coursesResponse = await _moodleIntegrationService.GetCoursesByFieldAsync(
                    config,
                    field,
                    value,
                    cancellationToken
                );              

                return (config.ShortName, coursesResponse, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error searching courses in instance {Instance}",
                    config.ShortName
                );

                return (config.ShortName, null, InstanceErrorHelper.CreateFromException(config.ShortName, ex));
            }
        }

        /// <summary>
        /// Aggregate category results from multiple instances
        /// </summary>
        private void AggregateCategoryResults(
            CategoriesResponse response,
            IEnumerable<(string ShortName, List<MoodleCategory>? Result, InstanceError? Error)> results)
        {
            foreach (var (shortName, categories, error) in results)
            {
                if (error != null)
                {
                    response.Errors.Add(error);
                }
                else if (categories != null)
                {
                    foreach (var category in categories)
                    {
                        response.Categories.Add(new MoodleCategory
                        {
                            Id = category.Id,
                            Name = category.Name,
                            Description = category.Description,
                            Parent = category.Parent,
                            Depth = category.Depth,
                            Path = category.Path,
                            Visible = category.Visible
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Aggregate course search results from multiple instances
        /// </summary>
        private void AggregateCourseResults(
            CoursesResponse response,
            IEnumerable<(string ShortName, MoodleCoursesResponseModel? Result, InstanceError? Error)> results)
        {
            foreach (var (shortName, courses, error) in results)
            {
                if (error != null)
                {
                    response.Errors.Add(error);
                }
                else if (courses != null)
                {
                    response.Courses = courses;
                }
            }
        }
    }
}
