using LearningHub.Nhs.Models.Moodle.API;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Payloads
{
    /// <summary>
    /// User certificate data from a single Moodle instance
    /// </summary>
    /// <remarks>
    /// Contains certificates returned from the mylearningservice_get_user_certificates Moodle web service API.
    /// </remarks>
    public class UserCertificatesPayload
    {
        /// <summary>
        /// List of certificates awarded to the user
        /// </summary>
        /// <remarks>
        /// Returns an empty list if the user has no certificates in this instance.
        /// </remarks>
        [JsonPropertyName("certificates")]
        public List<MoodleUserCertificateResponseModel> Certificates { get; set; } = new();
    }
}
