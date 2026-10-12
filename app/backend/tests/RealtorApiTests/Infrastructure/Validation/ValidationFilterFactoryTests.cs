using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Infrastructure.Validation;

namespace RealtorApiTests.Infrastructure.Validation;

public sealed class ValidationFilterFactoryTests
{
    [Fact]
    public async Task ValidateArgumentAsync_WithValidator_ReturnsBadRequestProblemDetails()
    {
        using var services = CreateServiceProvider(registerValidator: true);
        using var scope = services.CreateScope();

        var result = await ValidationFilterFactory.ValidateArgumentAsync(
            new TestRequest(string.Empty),
            typeof(TestRequest),
            scope.ServiceProvider,
            CancellationToken.None);

        Assert.NotNull(result);

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = scope.ServiceProvider;
        await using var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;

        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        Assert.StartsWith("application/problem+json", httpContext.Response.ContentType);

        responseBody.Position = 0;
        using var document = await JsonDocument.ParseAsync(responseBody);
        Assert.True(document.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty(nameof(TestRequest.Name), out _));
    }

    [Fact]
    public async Task ValidateArgumentAsync_WithoutValidator_PassesThrough()
    {
        using var services = CreateServiceProvider(registerValidator: false);
        using var scope = services.CreateScope();

        var result = await ValidationFilterFactory.ValidateArgumentAsync(
            new TestRequest(string.Empty),
            typeof(TestRequest),
            scope.ServiceProvider,
            CancellationToken.None);

        Assert.Null(result);
    }

    private static ServiceProvider CreateServiceProvider(bool registerValidator)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        if (registerValidator)
        {
            services.RegisterValidators(typeof(TestRequestValidator).Assembly);
        }

        return services.BuildServiceProvider();
    }

    public sealed record TestRequest(string Name);

    public sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(request => request.Name).NotEmpty();
        }
    }
}