namespace MoodleInstanceBridge.Models.Configuration
{
    /// <summary>
    /// Complete configuration for a Moodle instance including secrets
    /// </summary>
    public class MoodleInstanceConfig
    {
        /// <summary>
        /// Unique short name/identifier for the instance
        /// </summary>
        public required string ShortName { get; set; }

        /// <summary>
        /// Base URL of the Moodle instance
        /// </summary>
        public required string BaseUrl { get; set; }

        /// <summary>
        /// API token for authentication
        /// </summary>
        public required string ApiToken { get; set; }

        /// <summary>
        /// List of enabled endpoint names
        /// </summary>
        public IReadOnlyList<string> EnabledEndpoints { get; set; } = new List<string>();

        /// <summary>
        /// Weighting for load balancing or priority (0-100, higher is more weight)
        /// </summary>
        public int Weighting { get; set; } = 100;

        /// <summary>
        /// Whether this instance configuration is active
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}
