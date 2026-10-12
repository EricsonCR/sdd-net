using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace RealtorApi.Infrastructure.Endpoints;

public sealed class HealthEndpoint : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Microsoft.AspNetCore.Http.Results.Ok(new
        {
            status = "Healthy",
            service = "RealtorApi",
            timestamp = DateTimeOffset.UtcNow
        }));
    }
}