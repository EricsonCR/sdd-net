using Microsoft.EntityFrameworkCore;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests.Infrastructure.Persistence;

public class DatabaseSeederTests
{
    [Fact]
    public async Task SeedAsync_PersistsSeededPropertiesWithoutDuplicates()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);

        await DatabaseSeeder.SeedAsync(context);
        await DatabaseSeeder.SeedAsync(context);

        Assert.Equal(10, await context.Properties.CountAsync());
        Assert.Equal(10, await context.Properties.Select(property => property.Id).Distinct().CountAsync());
    }
}
