using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Features.Properties.ListProperties;

public sealed class ListPropertiesEndpoint : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/properties", HandleAsync)
            .AllowAnonymous();
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] ListPropertiesQuery query,
        ListPropertiesHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            query,
            httpContext.Request.Scheme,
            httpContext.Request.Host,
            cancellationToken);

        if (result.IsFailure)
        {
            var problemDetails = ProblemDetailsMapping.ToProblemDetails(result);
            return Results.Problem(
                detail: problemDetails.Detail,
                instance: problemDetails.Instance,
                statusCode: result.Error!.Status,
                title: problemDetails.Title,
                type: problemDetails.Type,
                extensions: problemDetails.Extensions);
        }

        return Results.Ok(result.Value);
    }
}