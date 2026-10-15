using Microsoft.EntityFrameworkCore;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests.Infrastructure.Persistence;

public class PropertyImageUrlTests
{
    [Fact]
    public async Task SeedAsync_UsesPublicImageRoute()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);

        await DatabaseSeeder.SeedAsync(context);

        var property = await context.Properties.FirstAsync();

        Assert.StartsWith("/images/properties/", property.ImageUrl);
        Assert.DoesNotContain("support", property.ImageUrl, StringComparison.OrdinalIgnoreCase);
    }
}
