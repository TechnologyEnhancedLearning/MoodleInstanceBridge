using Microsoft.Extensions.Options;
using MoodleInstanceBridge.Interfaces;
using MoodleInstanceBridge.Models.Configuration;
using System.Net.Http.Headers;

namespace MoodleInstanceBridge.Services.HttpClients
{

    /// <summary>
    /// The Moodle Http Client.
    /// </summary>
    public class MoodleHttpClient : IMoodleHttpClient, IDisposable
    {
        private readonly MoodleConfig moodleConfig;
        private readonly HttpClient httpClient = new();

        private bool initialised = false;
        private string moodleApiUrl;
        private string moodleApiMoodleWsRestFormat;
        private string moodleApiWsToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoodleHttpClient"/> class.
        /// </summary>
        /// <param name="httpClient">httpClient.</param>
        /// <param name="moodleConfig">config.</param>
        public MoodleHttpClient(HttpClient httpClient, IOptions<MoodleConfig> moodleConfig)
        {
            this.moodleConfig = moodleConfig.Value;
            this.httpClient = httpClient;

            this.moodleApiUrl = this.moodleConfig.ApiBaseUrl + this.moodleConfig.ApiPath;
            this.moodleApiMoodleWsRestFormat = this.moodleConfig.ApiWsRestFormat;
            this.moodleApiWsToken = this.moodleConfig.ApiWsToken;
        }

        /// <summary>
        /// The Get Client method.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task<HttpClient> GetClient()
        {
            this.Initialise(this.moodleApiUrl);
            return this.httpClient;
        }

        /// <summary>
        /// GetDefaultParameters.
        /// </summary>
        /// <returns>defaultParameters.</returns>
        public string GetDefaultParameters()
        {
            string defaultParameters = $"wstoken={this.moodleApiWsToken}"
                              + $"&moodlewsrestformat={this.moodleApiMoodleWsRestFormat}";

            return defaultParameters;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The dispoase.
        /// </summary>
        /// <param name="disposing">disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.httpClient.Dispose();
            }
        }

        private void Initialise(string httpClientUrl)
        {
            if (this.initialised == false)
            {
                this.httpClient.BaseAddress = new Uri(httpClientUrl);
                this.httpClient.DefaultRequestHeaders.Accept.Clear();
                this.httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                this.initialised = true;
            }
        }
    }
}
