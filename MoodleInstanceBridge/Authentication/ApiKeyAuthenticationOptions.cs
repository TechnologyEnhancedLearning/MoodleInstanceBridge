using Microsoft.AspNetCore.Authentication;

namespace MoodleInstanceBridge.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ApiKey";
        public string HeaderName { get; set; } = "X-API-KEY";
    }
}