using LearningHub.Nhs.Models.Moodle;
using LearningHub.Nhs.Models.Moodle.API;

namespace MoodleInstanceBridge.Services.Moodle
{
    public interface IMoodleApiService
    {
        /// <summary>
        /// GetAllMoodleCategoriesAsync.
        /// </summary>
        /// <returns>List of MoodleCategory.</returns>
        Task<List<MoodleCategory>> GetAllMoodleCategoriesAsync();

        /// <summary>
        /// GetCoursesByCategoryIdAsync.
        /// </summary>
        /// <param name="categoryId">The categoryId.</param>
        /// <returns> List of MoodleCoursesResponseModel.</returns>
        Task<MoodleCoursesResponseModel> GetCoursesByCategoryIdAsync(int categoryId);
    }
}
