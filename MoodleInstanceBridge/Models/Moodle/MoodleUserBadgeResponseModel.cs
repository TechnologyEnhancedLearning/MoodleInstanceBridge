using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Models.Moodle
{
    /// <summary>
    /// Represents a badge awarded to a Moodle user, as returned by core_badges_get_user_badges
    /// </summary>
    ///  [TODO] This needs to moved to the LearningHub.Nhs.Models.Moodle project, 
    ///  but is left here for now to avoid breaking changes in the API response model until we can do a full refactor of the Moodle models
    ///  Swapna aware of this and it's on the roadmap to be done in the future

    public class MoodleUserBadgeResponseModel
    {
        /// <summary>Badge ID</summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>Badge name</summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>Badge description</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>Unix timestamp of when the badge was created</summary>
        [JsonPropertyName("timecreated")]
        public int TimeCreated { get; set; }

        /// <summary>Unix timestamp of when the badge was last modified</summary>
        [JsonPropertyName("timemodified")]
        public int TimeModified { get; set; }

        /// <summary>Name of the badge issuer</summary>
        [JsonPropertyName("issuername")]
        public string? IssuerName { get; set; }

        /// <summary>URL of the badge issuer</summary>
        [JsonPropertyName("issuerurl")]
        public string? IssuerUrl { get; set; }

        /// <summary>Contact details of the badge issuer</summary>
        [JsonPropertyName("issuercontact")]
        public string? IssuerContact { get; set; }

        /// <summary>Unix timestamp of when the badge expires (null if no expiry date)</summary>
        [JsonPropertyName("expiredate")]
        public int? ExpireDate { get; set; }

        /// <summary>Expiry period in days (null if no expiry period)</summary>
        [JsonPropertyName("expireperiod")]
        public int? ExpirePeriod { get; set; }

        /// <summary>Badge type: 1 = site badge, 2 = course badge</summary>
        [JsonPropertyName("type")]
        public int Type { get; set; }

        /// <summary>Course ID associated with this badge (null for site badges)</summary>
        [JsonPropertyName("courseid")]
        public int? CourseId { get; set; }

        /// <summary>Badge status</summary>
        [JsonPropertyName("status")]
        public int Status { get; set; }

        /// <summary>Issued badge ID</summary>
        [JsonPropertyName("issuedid")]
        public int IssuedId { get; set; }

        /// <summary>Unique hash for the issued badge</summary>
        [JsonPropertyName("uniquehash")]
        public string? UniqueHash { get; set; }

        /// <summary>Unix timestamp of when the badge was issued</summary>
        [JsonPropertyName("dateissued")]
        public int DateIssued { get; set; }

        /// <summary>Unix timestamp of when the issued badge expires (null if no expiry)</summary>
        [JsonPropertyName("dateexpire")]
        public int? DateExpire { get; set; }

        /// <summary>Whether the badge is visible (1 = visible, 0 = hidden)</summary>
        [JsonPropertyName("visible")]
        public int Visible { get; set; }

        /// <summary>URL to view the issued badge</summary>
        [JsonPropertyName("badgeurl")]
        public string? BadgeUrl { get; set; }

        /// <summary>Badge version</summary>
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        /// <summary>Badge language code</summary>
        [JsonPropertyName("language")]
        public string? Language { get; set; }
    }

    /// <summary>
    /// Wrapper for the core_badges_get_user_badges API response
    /// </summary>
    public class MoodleUserBadgesApiResponse
    {
        /// <summary>List of badges awarded to the user</summary>
        [JsonPropertyName("badges")]
        public List<MoodleUserBadgeResponseModel> Badges { get; set; } = new();

        /// <summary>Any warnings returned by the API</summary>
        [JsonPropertyName("warnings")]
        public List<object> Warnings { get; set; } = new();
    }
}
