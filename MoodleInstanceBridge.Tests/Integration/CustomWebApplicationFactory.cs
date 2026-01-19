using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MoodleInstanceBridge.Interfaces;

namespace MoodleInstanceBridge.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // You can override services here for integration testing
            // For example, replace the real IInstanceConfigurationService with a test implementation
        });
    }
}
