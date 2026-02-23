using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using MoodleInstanceBridge.Models.Errors;


public class SwaggerConfig : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    private readonly IConfiguration _configuration;
    public SwaggerConfig(
         IApiVersionDescriptionProvider provider,
         IConfiguration configuration)
    {
        _provider = provider;
        _configuration = configuration;
    }

    public void Configure(SwaggerGenOptions options)
    {
        var authUrl = _configuration.GetValue<string>(
            "LearningHubAuthServiceConfig:AuthorizationUrl");

        var tokenUrl = _configuration.GetValue<string>(
            "LearningHubAuthServiceConfig:TokenUrl");
        // API Key definition
        options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Description = "Enter API Key in header: X-API-KEY",
            Name = "X-API-KEY",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    }
                },
                Array.Empty<string>()
            }
        });
        options.AddSecurityRequirement(
                        new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                        { Type = ReferenceType.SecurityScheme, Id = "ApiKey" },
                                },
                                new string[] { } // Must be empty since not oauth2 - see https://github.com/domaindrivendev/Swashbuckle.AspNetCore#add-security-definitions-and-requirements
                            },
                        });
        options.AddSecurityDefinition(
            "OAuth",
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                In = ParameterLocation.Header,
                Name = "Authorization",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(authUrl),
                        TokenUrl = new Uri(tokenUrl),
                        Scopes = new Dictionary<string, string>
                        {
                                        { "learninghubapi", string.Empty },

                        },
                    },
                },
            });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Id = "OAuth", Type = ReferenceType.SecurityScheme },
                            },
                            new string[] { }
                        },
                    });
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Moodle Instance Bridge API",
                Version = description.ApiVersion.ToString()
            });
        }
    }
}
