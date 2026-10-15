using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests.Infrastructure.Persistence;

public class PropertyConfigurationTests
{
    [Fact]
    public void PropertyStatus_IsStoredAsString()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = "Casa de prueba",
            Description = "Descripcion de prueba",
            Address = "Avenida de prueba 123",
            Price = 2500m,
            Status = PropertyStatus.Available,
            BedroomCount = 2,
            BathroomCount = 1,
            AreaSquareMeters = 120m,
            ImageUrl = "/images/properties/test.png",
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Properties.Add(property);
        context.SaveChanges();

        var saved = context.Properties.Single();
        Assert.Equal(PropertyStatus.Available, saved.Status);
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
        Assert.NotNull(statusProperty!.GetValueConverter());
        Assert.Equal(nameof(Property.Id), entityType.FindPrimaryKey()!.Properties.Single().Name);
    }
}
