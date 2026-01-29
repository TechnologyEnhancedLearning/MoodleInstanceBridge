using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Contracts.Aggregate
{
    /// <summary>
    /// Aggregated response from multiple Moodle instances
    /// </summary>
    /// <typeparam name="T">The type of data payload returned from each instance</typeparam>
    /// <remarks>
    /// Each result contains either data or an error. Use the <see cref="AggregateResult{T}.IsSuccess"/> 
    /// property to check if an instance returned data successfully. This allows partial failures where 
    /// some instances succeed while others fail.
    /// </remarks>
    public class AggregateResponse<T>
    {
        /// <summary>
        /// Results from each queried Moodle instance (may contain data or error per instance)
        /// </summary>
        [JsonPropertyName("results")]
        public List<AggregateResult<T>> Results { get; set; } = new();
    }
}
