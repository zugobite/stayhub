using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Core.Models;

namespace StayHub.Data.Configurations;

/// <summary>
/// Maps the host-defined check-in process for a listing.
/// </summary>
public class CheckInProcessConfiguration : IEntityTypeConfiguration<CheckInProcess>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CheckInProcess> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Title).HasMaxLength(200).IsRequired();
        builder.Property(c => c.StepsJson).IsRequired();
        builder.HasIndex(c => c.PropertyId).IsUnique();
    }
}

/// <summary>
/// Maps one guest's run through a check-in process.
/// </summary>
public class GuestCheckInConfiguration : IEntityTypeConfiguration<GuestCheckIn>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GuestCheckIn> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(25);

        builder.HasIndex(c => c.BookingId).IsUnique();

        builder.HasOne(c => c.CheckInProcess)
            .WithMany(p => p.GuestCheckIns)
            .HasForeignKey(c => c.CheckInProcessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Documents)
            .WithOne(d => d.GuestCheckIn)
            .HasForeignKey(d => d.GuestCheckInId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Maps uploaded guest identity documents.
/// </summary>
public class GuestDocumentConfiguration : IEntityTypeConfiguration<GuestDocument>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GuestDocument> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.DocumentType).HasMaxLength(100).IsRequired();
        builder.Property(d => d.FileName).HasMaxLength(200).IsRequired();
        builder.Property(d => d.FileUrl).HasMaxLength(500).IsRequired();
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
    }
}
