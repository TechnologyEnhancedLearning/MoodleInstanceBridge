using LearningHub.Nhs.Models.Moodle;
using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Contracts.Aggregate;
using MoodleInstanceBridge.Contracts.Errors;
using MoodleInstanceBridge.Contracts.Payloads;
using MoodleInstanceBridge.Interfaces;
using MoodleInstanceBridge.Interfaces.Services;
using MoodleInstanceBridge.Models.Configuration;
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
        private readonly MultiInstanceOrchestrator<List<MoodleSubCategoryResponseModel>> _subCategoryOrchestrator;
        private readonly IMoodleIntegrationService _moodleIntegrationService;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            MultiInstanceOrchestrator<List<MoodleCategory>> categoryOrchestrator,
            MultiInstanceOrchestrator<MoodleCoursesResponseModel> courseOrchestrator,
            MultiInstanceOrchestrator<List<MoodleSubCategoryResponseModel>> subCategoryOrchestrator,
            IMoodleIntegrationService moodleIntegrationService,
            ILogger<CourseService> logger)
        {
            _categoryOrchestrator = categoryOrchestrator;
            _courseOrchestrator = courseOrchestrator;
            _subCategoryOrchestrator = subCategoryOrchestrator;
            _moodleIntegrationService = moodleIntegrationService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<AggregateResponse<CategoriesPayload>> GetCategoriesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _categoryOrchestrator.ExecuteAcrossInstancesAsync(
                operationName: "Category lookup",
                instanceOperation: (config, ct) => GetCategoriesFromInstanceAsync(config, ct),
                resultAggregator: AggregateCategoryResults,
                createEmptyResponse: () => new AggregateResponse<CategoriesPayload>(),
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<AggregateResponse<CoursesPayload>> SearchCoursesAsync(
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
                createEmptyResponse: () => new AggregateResponse<CoursesPayload>(),
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<AggregateResponse<SubCategoriesPayload>> GetSubCategoriesAsync(
            int categoryId,
            string? instance = null,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(instance))
            {
                // Target a single named instance.
                // Capture config.ShortName inside the operation so the result label is consistent
                // with the multi-instance path (which uses config.ShortName).
                string? resolvedShortName = null;
                var singleResult = await _subCategoryOrchestrator.ExecuteOnInstanceAsync(
                    shortName: instance,
                    operation: async (config, ct) =>
                    {
                        resolvedShortName = config.ShortName;
                        return await _moodleIntegrationService.GetSubCategoriesAsync(config, categoryId, ct);
                    },
                    cancellationToken: cancellationToken
                );

                var (_, result, error) = singleResult;
                var normalizedResult = (resolvedShortName ?? instance, result, error);

                var response = new AggregateResponse<SubCategoriesPayload>();
                AggregateSubCategoryResults(response, new[] { normalizedResult });
                return response;
            }

            return await _subCategoryOrchestrator.ExecuteAcrossInstancesAsync(
                operationName: $"Subcategory lookup for category {categoryId}",
                instanceOperation: (config, ct) => GetSubCategoriesFromInstanceAsync(config, categoryId, config.ShortName, ct),
                resultAggregator: AggregateSubCategoryResults,
                createEmptyResponse: () => new AggregateResponse<SubCategoriesPayload>(),
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
        /// Get subcategories from a specific Moodle instance - domain logic only
        /// </summary>
        private async Task<(string ShortName, List<MoodleSubCategoryResponseModel>? Result, InstanceError? Error)>
            GetSubCategoriesFromInstanceAsync(
                MoodleInstanceConfig config,
                int categoryId,
                string instance,
                CancellationToken cancellationToken)
        {
            try
            {
                var subcategories = await _moodleIntegrationService.GetSubCategoriesAsync(
                    config,
                    categoryId,
                    cancellationToken
                );

                return (instance, subcategories, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting subcategories for category {CategoryId} from instance {Instance}",
                    categoryId,
                    instance
                );

                return (instance, null, InstanceErrorHelper.CreateFromException(instance, ex));
            }
        }

        /// <summary>
        /// Aggregate category results from multiple instances into an AggregateResponse
        /// </summary>
        private void AggregateCategoryResults(
            AggregateResponse<CategoriesPayload> response,
            IEnumerable<(string ShortName, List<MoodleCategory>? Result, InstanceError? Error)> results)
        {
            foreach (var (shortName, categories, error) in results)
            {
                if (error != null)
                {
                    response.Results.Add(new AggregateResult<CategoriesPayload>
                    {
                        Instance = shortName,
                        Error = error
                    });
                }
                else if (categories != null)
                {
                    response.Results.Add(new AggregateResult<CategoriesPayload>
                    {
                        Instance = shortName,
                        Data = new CategoriesPayload { Categories = categories }
                    });
                }
            }
        }

        /// <summary>
        /// Aggregate course search results from multiple instances into an AggregateResponse
        /// </summary>
        private void AggregateCourseResults(
            AggregateResponse<CoursesPayload> response,
            IEnumerable<(string ShortName, MoodleCoursesResponseModel? Result, InstanceError? Error)> results)
        {
            foreach (var (shortName, courses, error) in results)
            {
                if (error != null)
                {
                    response.Results.Add(new AggregateResult<CoursesPayload>
                    {
                        Instance = shortName,
                        Error = error
                    });
                }
                else if (courses != null)
                {
                    response.Results.Add(new AggregateResult<CoursesPayload>
                    {
                        Instance = shortName,
                        Data = new CoursesPayload { Courses = courses }
                    });
                }
            }
        }

        /// <summary>
        /// Aggregate subcategory results from multiple instances into an AggregateResponse
        /// </summary>
        private void AggregateSubCategoryResults(
            AggregateResponse<SubCategoriesPayload> response,
            IEnumerable<(string ShortName, List<MoodleSubCategoryResponseModel>? Result, InstanceError? Error)> results)
        {
            foreach (var (shortName, subcategories, error) in results)
            {
                if (error != null)
                {
                    response.Results.Add(new AggregateResult<SubCategoriesPayload>
                    {
                        Instance = shortName,
                        Error = error
                    });
                }
                else if (subcategories != null)
                {
                    response.Results.Add(new AggregateResult<SubCategoriesPayload>
                    {
                        Instance = shortName,
                        Data = new SubCategoriesPayload { SubCategories = subcategories }
                    });
                }
            }
        }
    }
}
