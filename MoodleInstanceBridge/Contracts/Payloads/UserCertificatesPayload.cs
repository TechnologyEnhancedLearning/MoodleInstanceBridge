using LearningHub.Nhs.Models.Moodle.API;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Payloads
{
    /// <summary>
    /// Certificate data from all instances
    /// </summary>
    public class UserCertificatesPayload
    {
        [JsonPropertyName("certificates")]
        public List<MoodleUserCertificateResponseModel> Certificates { get; set; } = new();
    }
}
