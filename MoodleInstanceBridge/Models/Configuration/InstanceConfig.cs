namespace MoodleInstanceBridge.Models.Configuration
{
    /// <summary>
    /// Database entity representing configuration for a Moodle instance
    /// </summary>
    public class InstanceConfig
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique short name/identifier for the instance
        /// </summary>
        public required string ShortName { get; set; }

        /// <summary>
        /// Base URL of the Moodle instance
        /// </summary>
        public required string BaseUrl { get; set; }

        /// <summary>
        /// Comma-separated list of enabled endpoint names (e.g., "users,courses,grades")
        /// </summary>
        public string? EnabledEndpoints { get; set; }

        /// <summary>
        /// Weighting for load balancing or priority (0-100, higher is more weight)
        /// </summary>
        public int Weighting { get; set; } = 100;

        /// <summary>
        /// Whether this instance configuration is active
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// When this configuration was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this configuration was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Name of the Key Vault secret containing the API token
        /// </summary>
        public required string TokenSecretName { get; set; }
    }
}
