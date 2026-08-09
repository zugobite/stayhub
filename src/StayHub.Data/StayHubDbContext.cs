using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StayHub.Core.Models;

namespace StayHub.Data;

/// <summary>
/// The EF Core unit of work for StayHub. Inherits the ASP.NET Core Identity
/// schema so users, roles, and claims live alongside the domain tables.
/// </summary>
public class StayHubDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="StayHubDbContext"/> class.
    /// </summary>
    /// <param name="options">The context options supplied by dependency injection.</param>
    public StayHubDbContext(DbContextOptions<StayHubDbContext> options)
        : base(options)
    {
    }

    /// <summary>Gets or sets the property listings.</summary>
    public DbSet<HostProperty> Properties => Set<HostProperty>();

    /// <summary>Gets or sets the listing photographs.</summary>
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();

    /// <summary>Gets or sets the amenities.</summary>
    public DbSet<Amenity> Amenities => Set<Amenity>();

    /// <summary>Gets or sets the property-to-amenity join rows.</summary>
    public DbSet<PropertyAmenity> PropertyAmenities => Set<PropertyAmenity>();

    /// <summary>Gets or sets the bookings.</summary>
    public DbSet<Booking> Bookings => Set<Booking>();

    /// <summary>Gets or sets the payments.</summary>
    public DbSet<Payment> Payments => Set<Payment>();

    /// <summary>Gets or sets the reviews.</summary>
    public DbSet<Review> Reviews => Set<Review>();

    /// <summary>Gets or sets the saved listings.</summary>
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    /// <summary>Gets or sets the inbox messages.</summary>
    public DbSet<Message> Messages => Set<Message>();

    /// <summary>Gets or sets the audit trail.</summary>
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    /// <summary>Gets or sets the notifications.</summary>
    public DbSet<Notification> Notifications => Set<Notification>();

    /// <summary>Gets or sets the host applications.</summary>
    public DbSet<HostApplication> HostApplications => Set<HostApplication>();

    /// <summary>Gets or sets the host-defined check-in processes.</summary>
    public DbSet<CheckInProcess> CheckInProcesses => Set<CheckInProcess>();

    /// <summary>Gets or sets the guest check-ins.</summary>
    public DbSet<GuestCheckIn> GuestCheckIns => Set<GuestCheckIn>();

    /// <summary>Gets or sets the uploaded identity documents.</summary>
    public DbSet<GuestDocument> GuestDocuments => Set<GuestDocument>();

    /// <summary>Gets the newsletter subscription set.</summary>
    public DbSet<NewsletterSubscription> NewsletterSubscriptions => Set<NewsletterSubscription>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Every IEntityTypeConfiguration in this assembly is applied automatically.
        builder.ApplyConfigurationsFromAssembly(typeof(StayHubDbContext).Assembly);
    }
}
