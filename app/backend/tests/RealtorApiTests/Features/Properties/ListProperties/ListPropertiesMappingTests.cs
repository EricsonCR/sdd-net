using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.ListProperties;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests.Features.Properties.ListProperties;

public class ListPropertiesMappingTests
{
    private static readonly string[] AllowedHosts = ["localhost", "127.0.0.1"];

    [Fact]
    public void CreateImageUrl_UsesAllowedRequestAuthorityAndPublicPath()
    {
        var result = ListPropertiesMapping.CreateImageUrl(
            "/images/properties/1.png",
            "http",
            new HostString("localhost", 5023),
            AllowedHosts);

        Assert.True(result.IsSuccess);
        Assert.Equal("http://localhost:5023/assets/properties/1.png", result.Value);
    }

    [Fact]
    public void CreateImageUrl_RejectsUnallowedHost()
    {
        var result = ListPropertiesMapping.CreateImageUrl(
            "1.png",
            "http",
            new HostString("not-allowed.invalid"),
            AllowedHosts);

        Assert.True(result.IsFailure);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Error!.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("C:\\private\\1.png")]
    [InlineData("/images/properties/../1.png")]
    public void CreateImageUrl_RejectsInvalidImagePath(string imagePath)
    {
        var result = ListPropertiesMapping.CreateImageUrl(
            imagePath,
            "http",
            new HostString("localhost", 5023),
            AllowedHosts);

        Assert.True(result.IsFailure);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.Error!.Status);
    }

    [Fact]
    public async Task ListPropertiesHandler_DoesNotReturnPartialItemsForInvalidImage()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new AppDbContext(options);
        var status = new PropertyStatus { Value = "Available", Description = "Disponible" };
        context.PropertyStatuses.Add(status);
        context.Properties.Add(new Property
        {
            Id = Guid.NewGuid(),
            Title = "Casa de prueba",
            Description = "Descripción",
            Address = "Dirección",
            Price = 100m,
            Status = status.Value,
            PropertyStatus = status,
            BedroomCount = 1,
            BathroomCount = 1,
            AreaSquareMeters = 50m,
            ImageUrl = "C:\\private\\1.png",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Properties:AllowedHosts"] = "localhost;127.0.0.1"
            })
            .Build();
        var handler = new ListPropertiesHandler(
            context,
            configuration,
            NullLogger<ListPropertiesHandler>.Instance);

        var result = await handler.HandleAsync(
            new ListPropertiesQuery(),
            "http",
            new HostString("localhost", 5023),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.Error!.Status);
        Assert.Null(result.Value);
    }
}