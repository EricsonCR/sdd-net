using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests.Infrastructure.Persistence;

public class PropertyConfigurationTests
{
    [Fact]
    public void PropertyStatus_IsStoredAsCatalogForeignKey()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var status = new PropertyStatus
        {
            Value = "Available",
            Description = "Disponible"
        };

        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = "Casa de prueba",
            Description = "Descripcion de prueba",
            Address = "Avenida de prueba 123",
            Price = 2500m,
            Status = status.Value,
            PropertyStatus = status,
            BedroomCount = 2,
            BathroomCount = 1,
            AreaSquareMeters = 120m,
            ImageUrl = "/images/properties/test.png",
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.PropertyStatuses.Add(status);
        context.Properties.Add(property);
        context.SaveChanges();

        var saved = context.Properties.Single();
        Assert.Equal("Available", saved.Status);
    }

    [Fact]
    public void Property_UsesExpectedEntityMetadata()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(Property));
        var statusProperty = entityType!.FindProperty(nameof(Property.Status));

        Assert.NotNull(entityType);
        Assert.NotNull(statusProperty);
        Assert.Null(statusProperty!.GetValueConverter());
        Assert.Equal(typeof(string), statusProperty.ClrType);
        Assert.Equal(nameof(Property.Id), entityType.FindPrimaryKey()!.Properties.Single().Name);

        var statusForeignKey = Assert.Single(entityType.GetForeignKeys());
        Assert.Equal(typeof(PropertyStatus), statusForeignKey.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(PropertyStatus.Value), Assert.Single(statusForeignKey.PrincipalKey.Properties).Name);
        Assert.Equal("property_status", statusForeignKey.PrincipalEntityType.GetTableName());
    }
}
