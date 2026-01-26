using LearningHub.Nhs.Models.Moodle.API;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Payloads
{
    /// <summary>
    /// Response containing course completion status from all Moodle instances
    /// </summary>
    public class CourseCompletionStatusPayload
    {
        [JsonPropertyName("completions")]
        public MoodleCourseCompletionModel Completions { get; set; } = new();
    }
}
