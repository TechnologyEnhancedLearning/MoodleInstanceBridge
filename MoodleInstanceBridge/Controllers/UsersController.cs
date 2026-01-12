using Microsoft.AspNetCore.Mvc;
using MoodleInstanceBridge.Attributes;
using MoodleInstanceBridge.Models.Errors;
using MoodleInstanceBridge.Models.Users;
using MoodleInstanceBridge.Services.Users;

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
        private readonly IUserLookupService _userLookupService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserLookupService userLookupService,
            ILogger<UsersController> logger)
        {
            _userLookupService = userLookupService;
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

            var response = await _userLookupService.GetMoodleUserIdsByEmailAsync(
                email,
                cancellationToken
            );

            return Ok(response);
        }
    }
}
