using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Core.Models;

namespace StayHub.Data.Configurations;

/// <summary>
/// Maps saved listings, one row per user and listing pair.
/// </summary>
public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.HasKey(w => w.Id);

        builder.HasIndex(w => new { w.UserId, w.PropertyId }).IsUnique();

        builder.HasOne(w => w.User)
            .WithMany(u => u.WishlistItems)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Property)
            .WithMany()
            .HasForeignKey(w => w.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
