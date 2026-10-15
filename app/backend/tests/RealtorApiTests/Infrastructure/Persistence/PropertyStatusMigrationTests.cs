using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests.Infrastructure.Persistence;

public class PropertyStatusMigrationTests
{
    [Fact]
    public void AddPropertyStatusCatalog_DownDropsOnlyCatalogRelationship()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=MigrationScriptOnly;Username=postgres;Password=postgres")
            .Options;

        using var context = new AppDbContext(options);
        var migrator = context.GetService<IMigrator>();

        var script = migrator.GenerateScript(
            fromMigration: "20261015081254_AddPropertyStatusCatalog",
            toMigration: "20261012105814_AddPropertyPersistence");

        Assert.Contains("FK_Properties_property_status_Status", script);
        Assert.Contains("property_status", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DROP TABLE \"Properties\"", script, StringComparison.OrdinalIgnoreCase);
    }
}