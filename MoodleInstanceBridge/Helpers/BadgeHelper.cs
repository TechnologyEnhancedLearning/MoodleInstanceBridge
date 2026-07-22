namespace MoodleInstanceBridge.Helpers
{
    /// <summary>
    /// Helper class for constructing Moodle badge URLs
    /// </summary>
    public static class BadgeHelper
    {
        private static readonly string BadgePath = "/badges/overview.php";

        /// <summary>
        /// Constructs a full badge URL from base URL and badge parameters
        /// </summary>
        /// <param name="apiBaseUrl">Moodle instance base URL</param>
        /// <param name="badgeId">Badge ID</param>
        /// <param name="uniqueHash">Unique hash for the badge</param>
        /// <returns>Complete URL to view the badge, or null if parameters are invalid</returns>
        public static string? GetBadgeUrl(string? apiBaseUrl, int? badgeId, string? uniqueHash)
        {
            if (string.IsNullOrWhiteSpace(apiBaseUrl) || !badgeId.HasValue || badgeId.Value <= 0 || string.IsNullOrWhiteSpace(uniqueHash))
            {
                return null;
            }

            return $"{apiBaseUrl.TrimEnd('/')}{BadgePath}?id={badgeId}&hash={uniqueHash}";
        }
    }
}
