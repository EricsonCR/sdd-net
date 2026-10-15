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

        var statuses = await context.PropertyStatuses
            .OrderBy(status => status.Value)
            .ToListAsync();
        Assert.Equal(10, await context.Properties.CountAsync());
        Assert.Equal(10, await context.Properties.Select(property => property.Id).Distinct().CountAsync());
        Assert.Equal(new[] { "Available", "Maintenance", "Rented" }, statuses.Select(status => status.Value));
        Assert.All(await context.Properties.ToListAsync(), property =>
            Assert.Contains(statuses, status => status.Value == property.Status));
    }

    [Fact]
    public void ResolveSeedStatus_ThrowsForUnknownStatus()
    {
        var statuses = new Dictionary<string, RealtorApi.Domain.Properties.PropertyStatus>(StringComparer.Ordinal)
        {
            ["Available"] = new() { Value = "Available", Description = "Disponible" }
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            DatabaseSeeder.ResolveSeedStatus("Unknown", statuses, Guid.NewGuid()));

        Assert.Contains("Unknown", exception.Message);
    }
}
