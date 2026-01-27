using MoodleInstanceBridge.Models.Configuration;
using System.Web;

namespace MoodleInstanceBridge.Services.WebServiceClient
{
    /// <summary>
    /// Helper class for building Moodle Web Service URLs
    /// </summary>
    public static class MoodleUrlBuilder
    {
        private const string WebServicePath = "/webservice/rest/server.php";
        private const string JsonFormat = "json";

        /// <summary>
        /// Builds a Moodle Web Service URL with the specified function and parameters
        /// </summary>
        /// <param name="config">Moodle instance configuration</param>
        /// <param name="wsFunction">Web service function name</param>
        /// <param name="additionalParams">Additional query parameters</param>
        /// <returns>Complete URL for the web service call</returns>
        public static string BuildUrl(
            MoodleInstanceConfig config,
            string wsFunction,
            Action<System.Collections.Specialized.NameValueCollection>? additionalParams = null)
        {
            var baseUrl = config.BaseUrl.TrimEnd('/');
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            
            queryParams["wstoken"] = config.ApiToken;
            queryParams["wsfunction"] = wsFunction;
            queryParams["moodlewsrestformat"] = JsonFormat;
            
            additionalParams?.Invoke(queryParams);
            
            return $"{baseUrl}{WebServicePath}?{queryParams}";
        }

        /// <summary>
        /// Builds URL for core_user_get_users_by_field
        /// </summary>
        public static string BuildUsersByFieldUrl(MoodleInstanceConfig config, string field, string value)
        {
            return BuildUrl(config, "core_user_get_users_by_field", queryParams =>
            {
                queryParams["field"] = field;
                queryParams["values[0]"] = value;
            });
        }

        /// <summary>
        /// Builds URL for core_course_get_categories
        /// </summary>
        public static string BuildCategoriesUrl(MoodleInstanceConfig config)
        {
            return BuildUrl(config, "core_course_get_categories");
        }

        /// <summary>
        /// Builds URL for core_course_get_courses_by_field
        /// </summary>
        public static string BuildCoursesByFieldUrl(MoodleInstanceConfig config, string field, string value)
        {
            return BuildUrl(config, "core_course_get_courses_by_field", queryParams =>
            {
                queryParams["field"] = field;
                queryParams["value"] = value;
            });
        }

        /// <summary>
        /// Builds URL for core_enrol_get_users_courses
        /// </summary>
        public static string BuildUserCoursesUrl(MoodleInstanceConfig config, int userId)
        {
            return BuildUrl(config, "core_enrol_get_users_courses", queryParams =>
            {
                queryParams["userid"] = userId.ToString();
            });
        }

        /// <summary>
        /// Builds URL for core_completion_get_course_completion_status
        /// </summary>
        public static string BuildCourseCompletionStatusUrl(MoodleInstanceConfig config, int userId, int courseId)
        {
            return BuildUrl(config, "core_completion_get_course_completion_status", queryParams =>
            {
                queryParams["userid"] = userId.ToString();
                queryParams["courseid"] = courseId.ToString();
            });
        }

        /// <summary>
        /// Builds URL for core_user_get_users
        /// </summary>
        public static string BuildGetUsersUrl(MoodleInstanceConfig config, int userId)
        {
            return BuildUrl(config, "core_user_get_users", queryParams =>
            {
                queryParams["criteria[0][key]"] = "id";
                queryParams["criteria[0][value]"] = userId.ToString();
            });
        }

        /// <summary>
        /// Builds URL for mylearningservice_get_recent_courses
        /// </summary>
        public static string BuildRecentCoursesUrl(MoodleInstanceConfig config, int userId)
        {
            return BuildUrl(config, "mylearningservice_get_recent_courses", queryParams =>
            {
                queryParams["userid"] = userId.ToString();
            });
        }

        /// <summary>
        /// Builds URL for mylearningservice_get_user_certificates
        /// </summary>
        public static string BuildUserCertificatesUrl(MoodleInstanceConfig config, int userId)
        {
            return BuildUrl(config, "mylearningservice_get_user_certificates", queryParams =>
            {
                queryParams["userid"] = userId.ToString();
            });
        }
    }
}
