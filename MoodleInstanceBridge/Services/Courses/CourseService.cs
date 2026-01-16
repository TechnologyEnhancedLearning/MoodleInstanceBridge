using LearningHub.Nhs.Models.Moodle;
using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Interfaces;
using MoodleInstanceBridge.Interfaces.Services;
using MoodleInstanceBridge.Models.Courses;
using MoodleInstanceBridge.Models.Users;

namespace MoodleInstanceBridge.Services.Courses
{
    /// <summary>
    /// Service for looking up course data across instances with parallel fan-out
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly IInstanceConfigurationService _configService;
        private readonly IMoodleIntegrationService _moodleClient;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            IInstanceConfigurationService configService,
            IMoodleIntegrationService moodleClient,
            ILogger<CourseService> logger)
        {
            _configService = configService;
            _moodleClient = moodleClient;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<CategoriesResponse> GetCategoriesAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Looking up categories from all Moodle instances");

            var response = new CategoriesResponse();

            // Get all enabled instance configurations
            var configurations = await _configService.GetAllConfigurationsAsync(cancellationToken);

            if (!configurations.Any())
            {
                _logger.LogWarning("No enabled Moodle instances configured");
                return response;
            }

            _logger.LogInformation(
                "Querying {Count} Moodle instances for categories",
                configurations.Count
            );

            // Create fan-out tasks for parallel execution
            var lookupTasks = configurations
                .Select(config => GetCategoriesFromInstanceAsync(config.ShortName, cancellationToken))
                .ToList();

            // Wait for all tasks to complete
            var results = await Task.WhenAll(lookupTasks);

            // Process results
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
                            //DescriptionFormat = category.DescriptionFormat,
                            //SortOrder = category.SortOrder,
                            //CourseCount = category.CourseCount,
                            Visible = category.Visible
                        });
                    }
                }
            }

            _logger.LogInformation(
                "Category lookup completed: {CategoryCount} categories from {SuccessCount} instances, {ErrorCount} errors",
                response.Categories.Count,
                results.Count(r => r.Error == null),
                response.Errors.Count
            );

            return response;
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

            _logger.LogInformation(
                "Searching for courses with {Field}={Value} across all Moodle instances",
                field,
                value
            );

            var response = new CoursesResponse();

            // Get all enabled instance configurations
            var configurations = await _configService.GetAllConfigurationsAsync(cancellationToken);

            if (!configurations.Any())
            {
                _logger.LogWarning("No enabled Moodle instances configured");
                return response;
            }

            _logger.LogInformation(
                "Querying {Count} Moodle instances for courses with {Field}={Value}",
                configurations.Count,
                field,
                value
            );

            // Create fan-out tasks for parallel execution
            var lookupTasks = configurations
                .Select(config => SearchCoursesInInstanceAsync(config.ShortName, field, value, cancellationToken))
                .ToList();

            // Wait for all tasks to complete
            var results = await Task.WhenAll(lookupTasks);

            // Process results
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

            //_logger.LogInformation(
            //    "Course search completed: {CourseCount} courses from {SuccessCount} instances, {ErrorCount} errors",
            //    response.Courses?.courses?.Count ?? 0,
            //    results.Count(r => r.Error == null),
            //    response.Errors.Count
            //);

            return response;
        }

        /// <summary>
        /// Get categories from a specific Moodle instance
        /// </summary>
        private async Task<(string ShortName, List<LearningHub.Nhs.Models.Moodle.MoodleCategory>? Categories, InstanceError? Error)> 
            GetCategoriesFromInstanceAsync(
                string shortName,
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

                // Call Moodle Web Service to get categories
                var categories = await _moodleClient.GetCategoriesAsync(config, cancellationToken);

                _logger.LogInformation(
                    "Found {Count} categories in instance {Instance}",
                    categories.Count,
                    shortName
                );

                return (shortName, categories, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting categories from instance {Instance}",
                    shortName
                );

                return (shortName, null, InstanceErrorHelper.CreateFromException(shortName, ex));
            }
        }

        /// <summary>
        /// Search for courses in a specific Moodle instance
        /// </summary>
        private async Task<(string ShortName, MoodleCoursesResponseModel? Courses, InstanceError? Error)> 
            SearchCoursesInInstanceAsync(
                string shortName,
                string field,
                string value,
                CancellationToken cancellationToken)
        {
            try
            {
                // Get the configuration for this instance
                var config = await _configService.GetConfigurationAsync(shortName, cancellationToken);
                if (config == null)
                {
                    _logger.LogError("Configuration not found for instance {Instance}", shortName);
                    return (shortName, null, InstanceErrorHelper.CreateConfigurationError(shortName));
                }

                // Call Moodle Web Service to search for courses
                var coursesResponse = await _moodleClient.GetCoursesByFieldAsync(
                    config,
                    field,
                    value,
                    cancellationToken
                );

                //_logger.LogInformation(
                //    "Found {Count} courses in instance {Instance}",
                //    coursesResponse.courses?.Count ?? 0,
                //    shortName
                //);

                return (shortName, coursesResponse, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error searching courses in instance {Instance}",
                    shortName
                );

                return (shortName, null, InstanceErrorHelper.CreateFromException(shortName, ex));
            }
        }
    }
}
