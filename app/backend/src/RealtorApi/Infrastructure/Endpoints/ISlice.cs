using Microsoft.AspNetCore.Routing;

namespace RealtorApi.Infrastructure.Endpoints;

public interface ISlice
{
    void AddEndpoint(IEndpointRouteBuilder app);
}