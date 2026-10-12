using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Infrastructure.Endpoints;

namespace RealtorApiTests.Infrastructure.Endpoints;

public sealed class SliceRegistrationTests
{
    [Fact]
    public void RegisterSlices_RegistersAssemblySlicesOnlyOnce()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestSlice).Assembly;

        services.RegisterSlices(assembly);
        services.RegisterSlices(assembly);

        using var serviceProvider = services.BuildServiceProvider();
        var registeredSlices = serviceProvider.GetServices<ISlice>();

        Assert.Equal(1, registeredSlices.Count(slice => slice.GetType() == typeof(TestSlice)));
    }

    public sealed class TestSlice : ISlice
    {
        public void AddEndpoint(IEndpointRouteBuilder app)
        {
        }
    }
}