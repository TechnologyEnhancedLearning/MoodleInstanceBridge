using LearningHub.Nhs.Models.Moodle;
using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MoodleInstanceBridge.Services.Moodle
{
    public class MoodleApiService
    {
        private readonly IMoodleHttpClient moodleHttpClient;
        private readonly ILogger<MoodleApiService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoodleApiService"/> class.
        /// </summary>
        /// <param name="moodleHttpClient">moodleHttpClient.</param>
        /// <param name="logger">logger.</param>
        public MoodleApiService(IMoodleHttpClient moodleHttpClient, ILogger<MoodleApiService> logger)
        {
            this.moodleHttpClient = moodleHttpClient;
            this._logger = logger;
        }

        // <summary>
        /// GetAllMoodleCategoriesAsync.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<List<MoodleCategory>> GetAllMoodleCategoriesAsync()
        {
            try
            {
                var parameters = new Dictionary<string, string>
                {

                };

                var categories = await GetCallMoodleApiAsync<List<MoodleCategory>>(
                    "core_course_get_categories",
                    parameters
                );

                if (categories == null || categories.Count == 0)
                    return new List<MoodleCategory>();

                return categories.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// GetCoursesByCategoryIdAsync.
        /// </summary>
        /// <param name="categoryId">The categoryId.</param>
        /// <returns></returns>
        public async Task<MoodleCoursesResponseModel> GetCoursesByCategoryIdAsync(int categoryId)
        {
            try
            {
                var parameters = new Dictionary<string, string>
            {
                { "field", "category" },
                { "value", categoryId.ToString() }
            };
                // Fetch courses by category id
                var courses = await GetCallMoodleApiAsync<MoodleCoursesResponseModel>(
                    "core_course_get_courses_by_field",
                    parameters
                );

                if (courses == null)
                    return JsonConvert.DeserializeObject<MoodleCoursesResponseModel>(JsonConvert.SerializeObject(courses));
                return courses;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<T> GetCallMoodleApiAsync<T>(string wsFunction, Dictionary<string, string> parameters)
        {
            var client = await this.moodleHttpClient.GetClient();
            string defaultParameters = this.moodleHttpClient.GetDefaultParameters();

            // Build URL query string
            var queryBuilder = new StringBuilder($"&wsfunction={wsFunction}");
            foreach (var param in parameters)
            {
                queryBuilder.Append($"&{param.Key}={Uri.EscapeDataString(param.Value)}");
            }

            string fullUrl = "?" + defaultParameters + queryBuilder.ToString();

            HttpResponseMessage response = await client.GetAsync(fullUrl);
            string result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Moodle may still return an error with 200 OK
                try
                {
                    using var document = JsonDocument.Parse(result);
                    var root = document.RootElement;

                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("exception", out var exceptionProp))
                    {
                        string? message = root.TryGetProperty("message", out var messageProp)
                            ? messageProp.GetString()
                            : "Unknown error";

                        this._logger.LogError($"Moodle returned an exception: {exceptionProp.GetString()}, Message: {message}");
                        throw new Exception($"Moodle API Error: {exceptionProp.GetString()}, Message: {message}");
                    }
                }
                catch (System.Text.Json.JsonException ex)
                {
                    this._logger.LogError(ex, "Failed to parse Moodle API response as JSON.");
                    throw;
                }

                var deserialized = JsonConvert.DeserializeObject<T>(result);

                return deserialized == null
                    ? throw new Exception($"Failed to deserialize Moodle API response into type {typeof(T).Name}. Raw response: {result}")
                    : deserialized;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                this._logger.LogError($"Moodle API access denied. Status Code: {response.StatusCode}");
                throw new Exception("AccessDenied to MoodleApi");
            }
            else
            {
                this._logger.LogError($"Moodle API error. Status Code: {response.StatusCode}, Response: {result}");
                throw new Exception("Error with MoodleApi");
            }
        }
    }
}
