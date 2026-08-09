using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Core.Models;

namespace StayHub.Data.Configurations;

/// <summary>
/// Maps the audit trail.
/// </summary>
public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Action).HasMaxLength(100).IsRequired();
        builder.Property(l => l.ActivityType).HasConversion<string>().HasMaxLength(30);
        builder.Property(l => l.Details).HasMaxLength(500);

        builder.HasIndex(l => l.CreatedAt);

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
