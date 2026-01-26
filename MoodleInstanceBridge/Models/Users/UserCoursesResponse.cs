using LearningHub.Nhs.Models.Moodle.API;
using MoodleInstanceBridge.Models.Errors;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Models.Users
{
    /// <summary>
    /// Response containing user's enrolled courses from all Moodle instances
    /// </summary>
    public class UserCoursesResponse
    {
        /// <summary>
        /// Courses data from all instances
        ///// </summary>
        //[JsonPropertyName("courses")]
        //public List<MoodleCourse> Courses { get; set; } = new();

        ///// <summary>
        ///// List of errors from instances that failed
        ///// </summary>
        //[JsonPropertyName("errors")]
        //public List<InstanceError> Errors { get; set; } = new();
    }
}
