using MoodleInstanceBridge.Models.Configuration;
using MoodleInstanceBridge.Models.Errors;
using MoodleInstanceBridge.Models.Moodle;
using System.Text.Json;
using System.Web;

namespace MoodleInstanceBridge.Services.Moodle
{
    /// <summary>
    /// Client for interacting with Moodle Web Services
    /// </summary>
    public class MoodleClient : IMoodleClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MoodleClient> _logger;
        private const int DefaultTimeoutSeconds = 30;

        public MoodleClient(IHttpClientFactory httpClientFactory, ILogger<MoodleClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<List<MoodleUser>> GetUsersByFieldAsync(
            MoodleInstanceConfig config,
            string field,
            string value,
            CancellationToken cancellationToken = default)
        {
            ValidateInputs(config, field, value);

            try
            {
                var url = BuildWebServiceUrl(config, field, value);
                var content = await ExecuteWebServiceRequestAsync(config, url, field, value, cancellationToken);
                ValidateMoodleResponse(config, content);
                return DeserializeUserResponse(config, content, field, value);
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
        /// Validates input parameters
        /// </summary>
        private static void ValidateInputs(MoodleInstanceConfig config, string field, string value)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentException("Field cannot be null or whitespace.", nameof(field));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
        }

        /// <summary>
        /// Builds the Moodle Web Service URL with query parameters
        /// </summary>
        /// TODO [BY]: Consider moving URL construction to a separate utility class if it grows more complex
        private static string BuildWebServiceUrl(MoodleInstanceConfig config, string field, string value)
        {
            var baseUrl = config.BaseUrl.TrimEnd('/');
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["wstoken"] = config.ApiToken;
            queryParams["wsfunction"] = "core_user_get_users_by_field";
            queryParams["moodlewsrestformat"] = "json";
            queryParams["field"] = field;
            queryParams["values[0]"] = value;

            return $"{baseUrl}/webservice/rest/server.php?{queryParams}";
        }

        /// <summary>
        /// Executes the HTTP request to Moodle Web Service
        /// </summary>
        private async Task<string> ExecuteWebServiceRequestAsync(
            MoodleInstanceConfig config,
            string url,
            string field,
            string value,
            CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(DefaultTimeoutSeconds);

            _logger.LogDebug(
                "Calling Moodle Web Service for instance {Instance}: {Function} with {Field}={Value}",
                config.ShortName,
                "core_user_get_users_by_field", // TODO [BY]: Consider passing function name as parameter if this method is reused
                field,
                value
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
        /// Deserializes the user response from JSON
        /// </summary>
        private List<MoodleUser> DeserializeUserResponse(
            MoodleInstanceConfig config,
            string content,
            string field,
            string value)
        {
            var users = JsonSerializer.Deserialize<List<MoodleUser>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation(
                "Found {Count} users for {Field}={Value} in instance {Instance}",
                users?.Count ?? 0,
                field,
                value,
                config.ShortName
            );

            return users ?? new List<MoodleUser>();
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
