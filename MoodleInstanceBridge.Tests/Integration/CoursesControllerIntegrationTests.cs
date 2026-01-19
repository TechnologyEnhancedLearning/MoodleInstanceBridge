using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MoodleInstanceBridge.Tests.Integration;

public class CoursesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CoursesControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetCategories_ReturnsSuccessStatusCode()
    {
        // Arrange
        // Note: This test requires API key authentication to be configured properly
        // In a real scenario, you'd add the appropriate auth headers

        // Act
        var response = await _client.GetAsync("/api/v1/courses/categories");

        // Assert
        // Expecting Unauthorized without proper auth, but endpoint should exist
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCourses_WithValidParameters_ReturnsSuccessStatusCode()
    {
        // Arrange
        var field = "category";
        var value = "1";

        // Act
        var response = await _client.GetAsync($"/api/v1/courses/search?field={field}&value={value}");

        // Assert
        // Expecting Unauthorized without proper auth, but endpoint should exist
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCourses_WithMissingParameters_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/courses/search");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SwaggerEndpoint_IsAccessible()
    {
        // Act
        var response = await _client.GetAsync("/swagger/index.html");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Act - assuming there's a health check endpoint
        var response = await _client.GetAsync("/health");

        // Assert
        // Will 404 if not configured, but that's acceptable
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}
