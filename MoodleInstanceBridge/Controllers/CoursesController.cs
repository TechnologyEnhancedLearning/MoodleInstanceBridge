using Microsoft.AspNetCore.Mvc;
using MoodleInstanceBridge.Attributes;
using MoodleInstanceBridge.Interfaces.Services;
using MoodleInstanceBridge.Models.Courses;
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
        /// Each category includes sourceInstance to identify which Moodle instance it came from.
        /// Partial failures are included in the errors array.
        /// </remarks>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(CategoriesResponse), StatusCodes.Status200OK)]
        [StandardErrorResponses]
        public async Task<ActionResult<CategoriesResponse>> GetCategories(
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
        /// Each course includes sourceInstance to identify which Moodle instance it came from.
        /// Partial failures are included in the errors array.
        /// 
        /// Common field values:
        /// - "category": Search by category ID
        /// - "id": Search by course ID
        /// - "shortname": Search by course short name
        /// </remarks>
        [HttpGet("search")]
        [ProducesResponseType(typeof(CoursesResponse), StatusCodes.Status200OK)]
        [ValidationErrorResponse]
        [StandardErrorResponses]
        public async Task<ActionResult<CoursesResponse>> SearchCourses(
            [FromQuery] string field,
            [FromQuery] string value,
            CancellationToken cancellationToken)
        {
            // Validate parameters
            if (string.IsNullOrWhiteSpace(field))
            {
                throw new ValidationException("field", "Field parameter is required.");
            }

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
    }
}
