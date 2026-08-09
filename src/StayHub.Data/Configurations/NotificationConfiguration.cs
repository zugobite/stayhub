using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Core.Models;

namespace StayHub.Data.Configurations;

/// <summary>
/// Maps notification-centre entries.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title).HasMaxLength(100).IsRequired();
        builder.Property(n => n.Body).HasMaxLength(1000);
        builder.Property(n => n.ActionUrl).HasMaxLength(300);
        builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(40);

        builder.HasIndex(n => new { n.UserId, n.IsRead });

        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
