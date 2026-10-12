using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Infrastructure.Validation;

namespace RealtorApi.Infrastructure.Endpoints;

public static class SliceEndpointExtensions
{
    public static IEndpointRouteBuilder MapSliceEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup(string.Empty);
        group.AddEndpointFilterFactory(ValidationFilterFactory.Create);
        foreach (var slice in app.ServiceProvider.GetServices<ISlice>())
        {
            slice.AddEndpoint(group);
        }

        return app;
    }
}