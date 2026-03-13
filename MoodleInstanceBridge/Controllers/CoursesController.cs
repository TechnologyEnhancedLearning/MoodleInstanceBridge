using Microsoft.AspNetCore.Mvc;
using MoodleInstanceBridge.Attributes;
using MoodleInstanceBridge.Contracts.Aggregate;
using MoodleInstanceBridge.Contracts.Payloads;
using MoodleInstanceBridge.Interfaces.Services;
using MoodleInstanceBridge.Models.Errors;

namespace MoodleInstanceBridge.Controllers
{
    /// <summary>
    /// Controller for course-related operations across Moodle instances
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseLookupService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(
            ICourseService courseLookupService,
            ILogger<CoursesController> logger)
        {
            _courseLookupService = courseLookupService;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories from all enabled Moodle instances
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Categories from all instances with source instance information</returns>
        /// <remarks>
        /// This endpoint queries all enabled Moodle instances and returns categories from each.
        /// Each result includes instance to identify which Moodle instance it came from.
        /// Partial failures are included per instance result with an error.
        /// </remarks>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(AggregateResponse<CategoriesPayload>), StatusCodes.Status200OK)]
        [StandardErrorResponses]
        public async Task<ActionResult<AggregateResponse<CategoriesPayload>>> GetCategories(
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received request to get categories from all Moodle instances");

            var response = await _courseLookupService.GetCategoriesAsync(cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Search for courses across all enabled Moodle instances
        /// </summary>
        /// <param name="field">Field to search by (e.g., "category" for courses in a category)</param>
        /// <param name="value">Value to search for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Courses from all instances matching the search criteria</returns>
        /// <remarks>
        /// This endpoint queries all enabled Moodle instances using core_course_get_courses_by_field.
        /// Each result includes instance to identify which Moodle instance it came from.
        /// Partial failures are included per instance result with an error.
        /// 
        /// Common field values:
        /// - "category": Search by category ID
        /// - "id": Search by course ID
        /// - "shortname": Search by course short name
        /// </remarks>
        [HttpGet("search")]
        [ProducesResponseType(typeof(AggregateResponse<CoursesPayload>), StatusCodes.Status200OK)]
        [ValidationErrorResponse]
        [StandardErrorResponses]
        public async Task<ActionResult<AggregateResponse<CoursesPayload>>> SearchCourses(
            [FromQuery] string value,
            CancellationToken cancellationToken)
        {
            string field = "category";

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ValidationException("value", "Value parameter is required.");
            }

            _logger.LogInformation(
                "Received request to search courses with {Field}={Value}",
                field,
                value
            );

            var response = await _courseLookupService.SearchCoursesAsync(
                field,
                value,
                cancellationToken
            );

            return Ok(response);
        }

        /// <summary>
        /// Get subcategories for a given category ID from all enabled Moodle instances
        /// </summary>
        /// <param name="categoryId">The parent category ID to retrieve subcategories for</param>
        /// <param name="instance">
        /// Optional short name of a specific Moodle instance to query.
        /// When omitted all enabled instances are queried.
        /// </param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Subcategories from all instances (or the specified instance) belonging to the specified parent category</returns>
        /// <remarks>
        /// This endpoint queries all enabled Moodle instances using core_course_get_categories
        /// with criteria[0][key]=id and criteria[0][value]={categoryId}, then filters to return
        /// only direct subcategories of the specified parent category.
        /// Each result includes instance to identify which Moodle instance it came from.
        /// Partial failures are included per instance result with an error.
        /// If no subcategories exist, an empty list is returned for each instance.
        /// Supply the optional <c>instance</c> query parameter to limit the query to a single named instance.
        /// </remarks>
        [HttpGet("{categoryId}/subcategories")]
        [ProducesResponseType(typeof(AggregateResponse<SubCategoriesPayload>), StatusCodes.Status200OK)]
        [StandardErrorResponses]
        public async Task<ActionResult<AggregateResponse<SubCategoriesPayload>>> GetSubCategories(
            [FromRoute] int categoryId,
            CancellationToken cancellationToken,
            [FromQuery] string? instance = null)
        {
            _logger.LogInformation(
                "Received request to get subcategories for category {CategoryId} from {Instance}",
                categoryId,
                instance ?? "all Moodle instances"
            );

            var response = await _courseLookupService.GetSubCategoriesAsync(categoryId, instance, cancellationToken);

            return Ok(response);
        }
    }
}
