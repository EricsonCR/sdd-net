using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Infrastructure.Persistence.Configurations;

public sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");

        builder.HasKey(property => property.Id);
        builder.Property(property => property.Id).ValueGeneratedNever();

        builder.Property(property => property.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(property => property.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(property => property.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(property => property.Price)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(property => property.Status)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasOne(property => property.PropertyStatus)
            .WithMany(status => status.Properties)
            .HasForeignKey(property => property.Status)
            .HasPrincipalKey(status => status.Value)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Properties_property_status_Status");

        builder.Property(property => property.BedroomCount)
            .IsRequired();

        builder.Property(property => property.BathroomCount)
            .IsRequired();

        builder.Property(property => property.AreaSquareMeters)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(property => property.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(property => property.CreatedAt)
            .IsRequired();

        builder.Property(property => property.UpdatedAt);
    }
}
