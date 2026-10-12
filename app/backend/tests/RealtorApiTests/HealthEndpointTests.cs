using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Infrastructure.Endpoints;

namespace RealtorApiTests;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthyStatus()
    {
        using var client = _factory.CreateClient();

        Assert.Single(_factory.Services.GetServices<ISlice>());

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var payload = document.RootElement;
        Assert.Equal("Healthy", payload.GetProperty("status").GetString());
        Assert.Equal("RealtorApi", payload.GetProperty("service").GetString());
        Assert.True(DateTimeOffset.TryParse(payload.GetProperty("timestamp").GetString(), out _));
    }
}
