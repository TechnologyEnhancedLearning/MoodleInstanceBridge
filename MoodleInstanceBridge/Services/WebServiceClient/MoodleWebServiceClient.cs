using MoodleInstanceBridge.Interfaces;
using MoodleInstanceBridge.Models.Configuration;
using MoodleInstanceBridge.Models.Errors;
using System.Text.Json;

namespace MoodleInstanceBridge.Services.WebServiceClient
{
    /// <summary>
    /// Generic client for executing Moodle Web Service requests
    /// Handles HTTP communication, error checking, and deserialization
    /// </summary>
    public class MoodleWebServiceClient : IMoodleWebServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MoodleWebServiceClient> _logger;
        private const int DefaultTimeoutSeconds = 30;

        public MoodleWebServiceClient(IHttpClientFactory httpClientFactory, ILogger<MoodleWebServiceClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<string> ExecuteRequestAsync(
            MoodleInstanceConfig config,
            string url,
            string functionName,
            CancellationToken cancellationToken = default)
        {
            ValidateConfig(config);

            try
            {
                var content = await ExecuteWebServiceRequestAsync(config, url, functionName, cancellationToken);
                ValidateMoodleResponse(config, content);
                return content;
            }
            catch (MoodleUpstreamException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw HandleException(ex, config.ShortName);
            }
        }

        /// <summary>       
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="url"></param>
        /// <param name="functionName">only used for logging and error reporting. </param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MoodleUpstreamException"></exception>
        /// </summary>
        public async Task<T> ExecuteRequestAsync<T>(
            MoodleInstanceConfig config,
            string url,
            string functionName,
            CancellationToken cancellationToken = default)
        {
            var content = await ExecuteRequestAsync(config, url, functionName, cancellationToken);
            
            var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? throw new MoodleUpstreamException(
                "Failed to deserialize Moodle response",
                config.ShortName);
        }

        /// <summary>
        /// Validates config parameter
        /// </summary>
        private static void ValidateConfig(MoodleInstanceConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Executes the HTTP request to Moodle Web Service
        /// </summary>
        private async Task<string> ExecuteWebServiceRequestAsync(
            MoodleInstanceConfig config,
            string url,
            string functionName,
            CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(DefaultTimeoutSeconds);

            _logger.LogDebug(
                "Calling Moodle Web Service for instance {Instance}: {Function}",
                config.ShortName,
                functionName
            );

            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Moodle instance {Instance} returned error status {StatusCode}: {Error}",
                    config.ShortName,
                    response.StatusCode,
                    errorContent
                );

                throw new MoodleUpstreamException(
                    $"Moodle instance returned error status {response.StatusCode}",
                    config.ShortName,
                    (int)response.StatusCode
                );
            }

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// Validates the response content for Moodle error objects
        /// </summary>
        private void ValidateMoodleResponse(MoodleInstanceConfig config, string content)
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(content);
                var root = jsonDoc.RootElement;

                // Check if response is a Moodle error object (not an array)
                if (root.ValueKind == JsonValueKind.Object &&
                    (root.TryGetProperty("exception", out _) || root.TryGetProperty("errorcode", out _)))
                {
                    var errorMessage = root.TryGetProperty("message", out var msgElement)
                        ? msgElement.GetString() ?? "Unknown Moodle error"
                        : "Unknown Moodle error";

                    _logger.LogError(
                        "Moodle instance {Instance} returned error response: {Response}",
                        config.ShortName,
                        content
                    );

                    throw new MoodleUpstreamException(
                        $"Moodle error: {errorMessage}",
                        config.ShortName
                    );
                }
            }
            catch (JsonException)
            {
                _logger.LogWarning(
                    "Failed to parse Moodle response as JSON for error checking in instance {Instance}",
                    config.ShortName
                );
            }
        }

        /// <summary>
        /// Maps exceptions to MoodleUpstreamException
        /// </summary>
        private MoodleUpstreamException HandleException(Exception ex, string instanceShortName)
        {
            _logger.LogError(ex, "Error calling Moodle instance {Instance}", instanceShortName);

            return ex switch
            {
                HttpRequestException httpEx => new MoodleUpstreamException(
                    "Failed to connect to Moodle instance",
                    httpEx,
                    instanceShortName),

                TaskCanceledException { InnerException: TimeoutException } => new MoodleUpstreamException(
                    "Request to Moodle instance timed out",
                    ex,
                    instanceShortName),

                _ => new MoodleUpstreamException(
                    $"Unexpected error calling Moodle instance: {ex.Message}",
                    ex,
                    instanceShortName)
            };
        }
    }
}
