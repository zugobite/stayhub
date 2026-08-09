using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Core.Models;

namespace StayHub.Data.Configurations;

/// <summary>
/// Maps host and additional-property applications.
/// </summary>
public class HostApplicationConfiguration : IEntityTypeConfiguration<HostApplication>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<HostApplication> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.ReviewNotes).HasMaxLength(500);

        builder.HasIndex(a => a.Status);

        builder.HasOne(a => a.ApplicationUser)
            .WithMany(u => u.HostApplications)
            .HasForeignKey(a => a.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Property)
            .WithMany()
            .HasForeignKey(a => a.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
