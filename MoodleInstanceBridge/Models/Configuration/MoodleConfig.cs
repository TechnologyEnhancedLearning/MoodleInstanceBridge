namespace MoodleInstanceBridge.Models.Configuration
{
    /// <summary>
    /// The Moodle Settings.
    /// </summary>
    public class MoodleConfig
    {
        /// <summary>
        /// Gets or sets the base url for the Moodle service.
        /// </summary>
        public string ApiBaseUrl { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Web service Rest Format.
        /// </summary>
        public string ApiWsRestFormat { get; set; } = null!;

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        public string ApiWsToken { get; set; } = null!;

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        public string ApiPath { get; set; } = "webservice/rest/server.php";

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        public string CoursePath { get; set; } = "course/view.php";
    }
}
