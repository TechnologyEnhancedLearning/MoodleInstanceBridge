using LearningHub.Nhs.Models.Moodle;
using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Interfaces;
using MoodleInstanceBridge.Interfaces.Services;
using MoodleInstanceBridge.Models.Configuration;

namespace MoodleInstanceBridge.Services.Moodle
{
    /// <summary>
    /// Integration service for Moodle.
    /// Exposes domain-level operations and delegates to underlying Moodle domain services and clients.
    /// </summary>

    public class MoodleIntegrationService : IMoodleIntegrationService
    {
        private readonly IMoodleUserService _userService;
        private readonly IMoodleCourseService _courseService;

        public MoodleIntegrationService(IMoodleUserService userService, IMoodleCourseService courseService)
        {
            _userService = userService;
            _courseService = courseService;
        }

        public Task<List<MoodleUser>> GetUsersByFieldAsync(
            MoodleInstanceConfig config,
            string field,
            string value,
            CancellationToken cancellationToken = default)
        {
            return _userService.GetUsersByFieldAsync(config, field, value, cancellationToken);
        }

        public Task<List<MoodleCategory>> GetCategoriesAsync(
            MoodleInstanceConfig config,
            CancellationToken cancellationToken = default)
        {
            return _courseService.GetCategoriesAsync(config, cancellationToken);
        }

        public Task<MoodleCoursesResponseModel> GetCoursesByFieldAsync(
            MoodleInstanceConfig config,
            string field,
            string value,
            CancellationToken cancellationToken = default)
        {
            return _courseService.GetCoursesByFieldAsync(config, field, value, cancellationToken);
        }

        public Task<MoodleCourseResponseModel> GetUserCoursesAsync(
            MoodleInstanceConfig config,
            int userId,
            CancellationToken cancellationToken = default)
        {
            return _userService.GetUserCoursesAsync(config, userId, cancellationToken);
        }

        public Task<MoodleCourseCompletionModel> GetCourseCompletionStatusAsync(
            MoodleInstanceConfig config,
            int userId,
            int courseId,
            CancellationToken cancellationToken = default)
        {
            return _userService.GetCourseCompletionStatusAsync(config, userId, courseId, cancellationToken);
        }

        public Task<MoodleUserResponseModel> GetUsersAsync(
            MoodleInstanceConfig config,
            int userId,
            CancellationToken cancellationToken = default)
        {
            return _userService.GetUsersAsync(config, userId, cancellationToken);
        }

        public Task<MoodleEnrolledCourseResponseModel> GetRecentCoursesAsync(
            MoodleInstanceConfig config,
            int userId,
            CancellationToken cancellationToken = default)
        {
            return _userService.GetRecentCoursesAsync(config, userId, cancellationToken);
        }

        public Task<MoodleUserCertificateResponseModel> GetUserCertificatesAsync(
            MoodleInstanceConfig config,
            int userId,
            CancellationToken cancellationToken = default)
        {
            return _userService.GetUserCertificatesAsync(config, userId, cancellationToken);
        }
    }
}
