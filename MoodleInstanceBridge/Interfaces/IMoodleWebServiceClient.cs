using MoodleInstanceBridge.Models.Configuration;

namespace MoodleInstanceBridge.Interfaces
{
    /// <summary>
    /// Generic client for executing Moodle Web Service requests
    /// </summary>
    public interface IMoodleWebServiceClient
    {
        /// <summary>
        /// Executes a Moodle Web Service request and returns the raw response content
        /// </summary>
        /// <param name="config">Moodle instance configuration</param>
        /// <param name="url">Complete URL for the web service call</param>
        /// <param name="functionName">Name of the Moodle function being called (for logging)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Raw JSON response content</returns>
        Task<string> ExecuteRequestAsync(
            MoodleInstanceConfig config,
            string url,
            string functionName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a Moodle Web Service request and deserializes the response
        /// </summary>
        /// <typeparam name="T">Type to deserialize the response to</typeparam>
        /// <param name="config">Moodle instance configuration</param>
        /// <param name="url">Complete URL for the web service call</param>
        /// <param name="functionName">Name of the Moodle function being called (for logging)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Deserialized response object</returns>
        Task<T> ExecuteRequestAsync<T>(
            MoodleInstanceConfig config,
            string url,
            string functionName,
            CancellationToken cancellationToken = default);
    }
}
