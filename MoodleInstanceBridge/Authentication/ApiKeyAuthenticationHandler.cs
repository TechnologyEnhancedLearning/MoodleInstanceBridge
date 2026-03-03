using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace MoodleInstanceBridge.Authentication
{
    public class ApiKeyAuthenticationHandler
        : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly IConfiguration _configuration;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration)
            : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // 1. Check header exists
            if (!Request.Headers.TryGetValue(Options.HeaderName, out var apiKeyHeaderValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("API key is empty"));
            }

            // 2. Read keys from config
            var validKeys = _configuration
                .GetSection("ApiKeys")
                .Get<Dictionary<string, string>>();

            if (validKeys == null || !validKeys.Any())
            {
                return Task.FromResult(AuthenticateResult.Fail("No API keys configured"));
            }

            // 3. Validate key
            var match = validKeys.FirstOrDefault(k => k.Value == providedApiKey);

            if (match.Equals(default(KeyValuePair<string, string>)))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));
            }

            // 4. Create authenticated identity
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, match.Key),
                new Claim("auth_type", "api_key")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}