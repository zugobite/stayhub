using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Core.Models;

namespace StayHub.Data.Configurations;

/// <summary>
/// Maps amenities and the join table that links them to listings.
/// </summary>
public class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Amenity> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasMaxLength(50).IsRequired();
        builder.Property(a => a.IconClass).HasMaxLength(50).IsRequired();
        builder.HasIndex(a => a.Name).IsUnique();
    }
}

/// <summary>
/// Maps the many-to-many join between listings and amenities.
/// </summary>
public class PropertyAmenityConfiguration : IEntityTypeConfiguration<PropertyAmenity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PropertyAmenity> builder)
    {
        builder.HasKey(pa => new { pa.PropertyId, pa.AmenityId });

        builder.HasOne(pa => pa.Property)
            .WithMany(p => p.PropertyAmenities)
            .HasForeignKey(pa => pa.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pa => pa.Amenity)
            .WithMany(a => a.PropertyAmenities)
            .HasForeignKey(pa => pa.AmenityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
