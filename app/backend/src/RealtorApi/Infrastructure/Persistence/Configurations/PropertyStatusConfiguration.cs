using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Infrastructure.Persistence.Configurations;

public sealed class PropertyStatusConfiguration : IEntityTypeConfiguration<PropertyStatus>
{
    public void Configure(EntityTypeBuilder<PropertyStatus> builder)
    {
        builder.ToTable("property_status", table =>
            table.HasCheckConstraint("CK_property_status_value_not_empty", "\"Value\" <> ''"));

        builder.HasKey(status => status.Value);

        builder.Property(status => status.Value)
            .HasMaxLength(32)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(status => status.Description)
            .IsRequired();
    }
}