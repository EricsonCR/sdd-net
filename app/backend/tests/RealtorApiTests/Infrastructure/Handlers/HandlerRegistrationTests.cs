using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Infrastructure.Handlers;

namespace RealtorApiTests.Infrastructure.Handlers;

public sealed class HandlerRegistrationTests
{
    [Fact]
    public void RegisterHandlers_RegistersConcreteHandlersOnce()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestHandler).Assembly;

        services.RegisterHandlers(assembly);
        services.RegisterHandlers(assembly);

        using var serviceProvider = services.BuildServiceProvider();
        var registeredHandlers = serviceProvider.GetServices<TestHandler>();

        Assert.Single(registeredHandlers);
    }

    public sealed class TestHandler : IHandler;
}