using Microsoft.AspNetCore.Mvc;
using MoodleInstanceBridge.Attributes;
using MoodleInstanceBridge.Contracts.Aggregate;
using MoodleInstanceBridge.Contracts.Payloads;
using MoodleInstanceBridge.Controllers.Helpers;
using MoodleInstanceBridge.Interfaces.Services;
using MoodleInstanceBridge.Models.Courses;
using MoodleInstanceBridge.Models.Errors;
using MoodleInstanceBridge.Models.Users;

namespace MoodleInstanceBridge.Controllers
{
    /// <summary>
    /// Controller for user-related operations
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userLookupService,
            ILogger<UsersController> logger)
        {
            _userService = userLookupService;
            _logger = logger;
        }

        /// <summary>
        /// Get Moodle user IDs per instance for a Learning Hub user by email
        /// </summary>
        /// <param name="email">Email address of the Learning Hub user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Moodle user IDs per instance</returns>
        /// <remarks>
        /// This endpoint queries all enabled Moodle instances and returns the corresponding
        /// Moodle user ID for each instance where the user exists. If the user does not exist
        /// in an instance, null is returned for that instance. Partial failures are included
        /// in the errors array.
        /// </remarks>
        [HttpGet("{email}/instance-ids")]
        [ProducesResponseType(typeof(MoodleUserIdsResponse), StatusCodes.Status200OK)]
        [ValidationErrorResponse]
        [StandardErrorResponses]
        public async Task<ActionResult<MoodleUserIdsResponse>> GetMoodleUserIdsByEmail(
            string email,
            CancellationToken cancellationToken)
        {
            // Validate email parameter
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidationException("email", "Email address is required.");
            }

            // Validate email format using MailAddress
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                // Ensure the parsed email matches the input (prevents some edge cases)
                if (!string.Equals(mailAddress.Address, email, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ValidationException("email", "Email address format is invalid.");
                }
            }
            catch (FormatException)
            {
                throw new ValidationException("email", "Email address format is invalid.");
            }

            _logger.LogInformation(
                "Received request to get Moodle user IDs for email: {Email}",
                email
            );

            var response = await _userService.GetMoodleUserIdsByEmailAsync(
                email,
                cancellationToken
            );

            return Ok(response);
        }

        /// <summary>
        /// Get courses for users across specified Moodle instances
        /// </summary>
        /// <param name="request">Request containing user IDs per instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User's enrolled courses from specified instances</returns>
        /// <remarks>
        /// This endpoint queries the specified Moodle instances and returns the courses
        /// the users are enrolled in. The request must include a map of instance IDs to user IDs.
        /// 
        /// Example request body:
        /// <code>
        /// {
        ///   "userIds": {
        ///     "mooc": 1234,
        ///     "vle": 982,
        ///     "lh-moodle": 5677
        ///   }
        /// }
        /// </code>
        /// 
        /// The response includes results from each instance. Each result contains either course data 
        /// or an error. This allows partial success when some instances fail.
        /// </remarks>
        [HttpPost("courses")]
        [ProducesResponseType(typeof(AggregateResponse<UserCoursePayload>), StatusCodes.Status200OK)]
        [ValidationErrorResponse]
        [StandardErrorResponses]
        public async Task<ActionResult<AggregateResponse<UserCoursePayload>>> GetUserCourses(
            [FromBody] UserIdsRequest request,
            CancellationToken cancellationToken)
        {
            RequestValidationHelper.ValidateUserIdsRequest(request);

            _logger.LogInformation(
                "Received request to get courses for {Count} instance(s)",
                request.UserIds.Count
            );

            var response = await _userService.GetUserCoursesAsync(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Get course completion status for users across specified Moodle instances
        /// </summary>
        /// <param name="courseId">Course ID to check completion for</param>
        /// <param name="request">Request containing user IDs per instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Course completion status from specified instances</returns>
        /// <remarks>
        /// Queries the specified Moodle instances for course completion status of a specific course.
        /// Returns completion criteria, progress tracking, and overall completion state per instance.
        /// 
        /// The response includes results from each instance. Each result contains either completion data 
        /// or an error. This allows partial success when some instances fail.
        /// </remarks>
        [HttpPost("course-completion/{courseId:int}")]
        [ProducesResponseType(typeof(AggregateResponse<CourseCompletionStatusPayload>), StatusCodes.Status200OK)]
        [ValidationErrorResponse]
        [StandardErrorResponses]
        public async Task<ActionResult<AggregateResponse<CourseCompletionStatusPayload>>> GetCourseCompletion(
            int courseId,
            [FromBody] UserIdsRequest request,
            CancellationToken cancellationToken)
        {
            if (courseId <= 0)
            {
                throw new ValidationException("courseId", "Course ID must be greater than zero.");
            }

            RequestValidationHelper.ValidateUserIdsRequest(request);

            _logger.LogInformation(
                "Received request to get course completion for course {CourseId} across {Count} instance(s)",
                courseId,
                request.UserIds.Count
            );

            var response = await _userService.GetCourseCompletionStatusAsync(request, courseId, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Get user data across specified Moodle instances
        /// </summary>
        /// <param name="request">Request containing user IDs per instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User data from specified instances</returns>
        /// <remarks>
        /// Retrieves user profile and account information from the specified Moodle instances.
        /// Includes profile details, preferences, and custom fields.
        /// 
        /// The response includes results from each instance. Each result contains either user data 
        /// or an error. This allows partial success when some instances fail.
        /// </remarks>
        [HttpPost("user-data")]
        [ProducesResponseType(typeof(AggregateResponse<UsersPayload>), StatusCodes.Status200OK)]
        [ValidationErrorResponse]
        [StandardErrorResponses]
        public async Task<ActionResult<AggregateResponse<UsersPayload>>> GetUsers(
            [FromBody] UserIdsRequest request,
            CancellationToken cancellationToken)
        {
            RequestValidationHelper.ValidateUserIdsRequest(request);

            _logger.LogInformation(
                "Received request to get user data for {Count} instance(s)",
                request.UserIds.Count
            );

            var response = await _userService.GetUsersAsync(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Get recent courses for users across specified Moodle instances
        /// </summary>
        /// <param name="request">Request containing user IDs per instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Recent courses from specified instances</returns>
        /// <remarks>
        /// Retrieves courses the users have recently accessed from the specified Moodle instances.
        /// Typically ordered by last access time with enrollment and progress details.
        /// 
        /// The response includes results from each instance. Each result contains either course data 
        /// or an error. This allows partial success when some instances fail.
        /// </remarks>
        [HttpPost("recent-courses")]
        [ProducesResponseType(typeof(AggregateResponse<RecentCoursesPayload>), StatusCodes.Status200OK)]
        [ValidationErrorResponse]
        [StandardErrorResponses]
        public async Task<ActionResult<AggregateResponse<RecentCoursesPayload>>> GetRecentCourses(
            [FromBody] UserIdsRequest request,
            CancellationToken cancellationToken)
        {
            RequestValidationHelper.ValidateUserIdsRequest(request);

            _logger.LogInformation(
                "Received request to get recent courses for {Count} instance(s)",
                request.UserIds.Count
            );

            var response = await _userService.GetRecentCoursesAsync(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Get user certificates across specified Moodle instances
        /// </summary>
        /// <param name="request">Request containing user IDs per instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User certificates from specified instances</returns>
        /// <remarks>
        /// Retrieves all certificates issued to users from the specified Moodle instances.
        /// Includes details about course completion and issuance dates.
        /// 
        /// The response includes results from each instance. Each result contains either certificate data 
        /// or an error. This allows partial success when some instances fail.
        /// </remarks>
        [HttpPost("certificates")]
        [ProducesResponseType(typeof(AggregateResponse<UserCertificatesPayload>), StatusCodes.Status200OK)]
        [ValidationErrorResponse]
        [StandardErrorResponses]
        public async Task<ActionResult<AggregateResponse<UserCertificatesPayload>>> GetCertificates(
            [FromBody] UserIdsRequest request,
            CancellationToken cancellationToken)
        {
            RequestValidationHelper.ValidateUserIdsRequest(request);

            _logger.LogInformation(
                "Received request to get certificates for {Count} instance(s)",
                request.UserIds.Count
            );

            var response = await _userService.GetUserCertificatesAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}
