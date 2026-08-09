using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Core.Models;

namespace StayHub.Data.Configurations;

/// <summary>
/// Maps the property listing, its owner, and its dependants.
/// </summary>
public class HostPropertyConfiguration : IEntityTypeConfiguration<HostProperty>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<HostProperty> builder)
    {
        builder.ToTable("Properties");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.Address).HasMaxLength(300).IsRequired();
        builder.Property(p => p.City).HasMaxLength(100).IsRequired();

        builder.Property(p => p.PricePerNight).HasColumnType("decimal(18,2)");
        builder.Property(p => p.CleaningFee).HasColumnType("decimal(18,2)");
        builder.Property(p => p.ServiceFeeRate).HasColumnType("decimal(5,4)");

        // Stored as text so the enums read plainly in SQL Server.
        builder.Property(p => p.PropertyType).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Province).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(p => p.City);
        builder.HasIndex(p => p.Province);
        builder.HasIndex(p => p.IsActive);

        // The gallery's default query filters on IsActive and sorts by price, so
        // cover both from one index.
        builder.HasIndex(p => new { p.IsActive, p.PricePerNight });

        builder.HasOne(p => p.Host)
            .WithMany(u => u.Properties)
            .HasForeignKey(p => p.HostId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Property)
            .HasForeignKey(i => i.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.CheckInProcess)
            .WithOne(c => c.Property)
            .HasForeignKey<CheckInProcess>(c => c.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
