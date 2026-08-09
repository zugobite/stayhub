using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Core.Models;

namespace StayHub.Data.Configurations;

/// <summary>
/// Maps bookings and their one-to-one payment and check-in.
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(b => b.Nights);

        builder.HasIndex(b => new { b.PropertyId, b.CheckInDate, b.CheckOutDate });

        builder.HasOne(b => b.Property)
            .WithMany(p => p.Bookings)
            .HasForeignKey(b => b.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restricted: the guest also owns properties and reviews, and SQL Server
        // rejects multiple cascade paths back to AspNetUsers.
        builder.HasOne(b => b.Guest)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Payment)
            .WithOne(p => p.Booking)
            .HasForeignKey<Payment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.GuestCheckIn)
            .WithOne(c => c.Booking)
            .HasForeignKey<GuestCheckIn>(c => c.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
