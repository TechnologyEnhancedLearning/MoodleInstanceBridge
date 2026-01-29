using MoodleInstanceBridge.Contracts.Errors;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Aggregate
{
    /// <summary>
    /// Result from a single Moodle instance, containing either data or an error
    /// </summary>
    /// <typeparam name="T">The type of data payload returned from the instance</typeparam>
    public class AggregateResult<T>
    {
        /// <summary>
        /// Instance short name identifier
        /// </summary>
        [JsonPropertyName("instance")]
        public string Instance { get; set; } = default!;

        /// <summary>
        /// Data from the instance (null if there was an error)
        /// </summary>
        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }

        /// <summary>
        /// Error from the instance (null if successful)
        /// </summary>
        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public InstanceError? Error { get; set; }

        /// <summary>
        /// Indicates whether this result contains data (true) or an error (false)
        /// </summary>
        [JsonIgnore]
        public bool IsSuccess => Error == null;
    }
}
