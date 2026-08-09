using StayHub.Core.Enums;

namespace StayHub.Core.Models;

/// <summary>
/// A lodging listing owned by a host. Encapsulates its own activation state and
/// the nightly pricing arithmetic used by the booking engine.
/// </summary>
public class HostProperty
{
    private bool _isActive;

    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the listing headline.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the long-form listing description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rate in ZAR charged per guest, per night. The booking
    /// subtotal therefore scales with party size as well as length of stay.
    /// </summary>
    public decimal PricePerNight { get; set; }

    /// <summary>Gets or sets the one-off cleaning fee in ZAR.</summary>
    public decimal CleaningFee { get; set; }

    /// <summary>
    /// Gets or sets the platform service fee as a fraction of the nightly subtotal
    /// (0.12 reproduces the R 372 fee on a R 3 100 stay shown in the brief).
    /// </summary>
    public decimal ServiceFeeRate { get; set; } = 0.12m;

    /// <summary>Gets or sets the street address.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Gets or sets the city, used by the guest gallery filter.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the province, used by the gallery's region filter. Coarser
    /// than <see cref="City"/>, so guests can browse a whole region at once.
    /// </summary>
    public Province Province { get; set; }

    /// <summary>Gets or sets the listing category, used by the guest gallery filter.</summary>
    public PropertyType PropertyType { get; set; }

    /// <summary>Gets or sets the maximum number of travellers.</summary>
    public int MaxGuests { get; set; } = 1;

    /// <summary>Gets or sets the bedroom count.</summary>
    public int Bedrooms { get; set; }

    /// <summary>Gets or sets the bed count.</summary>
    public int Beds { get; set; }

    /// <summary>Gets or sets the bathroom count.</summary>
    public int Bathrooms { get; set; }

    /// <summary>
    /// Gets the activation state. Only active listings appear in the guest gallery.
    /// Change it through <see cref="Activate"/> and <see cref="Deactivate"/>.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => _isActive = value; // Setter retained for EF Core materialisation.
    }

    /// <summary>Gets or sets the UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the owning host's identifier.</summary>
    public string HostId { get; set; } = string.Empty;

    /// <summary>Gets or sets the owning host.</summary>
    public ApplicationUser? Host { get; set; }

    /// <summary>Gets or sets the listing's photographs.</summary>
    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();

    /// <summary>Gets or sets the bookings placed against this listing.</summary>
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    /// <summary>Gets or sets the guest reviews of this listing.</summary>
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    /// <summary>Gets or sets the amenities offered by this listing.</summary>
    public ICollection<PropertyAmenity> PropertyAmenities { get; set; } = new List<PropertyAmenity>();

    /// <summary>Gets or sets the host-defined check-in process for this listing.</summary>
    public CheckInProcess? CheckInProcess { get; set; }

    /// <summary>
    /// Publishes the listing to the guest gallery.
    /// </summary>
    public void Activate() => _isActive = true;

    /// <summary>
    /// Withdraws the listing from the guest gallery without deleting it.
    /// </summary>
    public void Deactivate() => _isActive = false;

    /// <summary>
    /// Calculates the nightly subtotal before fees:
    /// <c>PricePerNight × Nights × Guests</c>.
    /// </summary>
    /// <param name="nights">The number of nights booked.</param>
    /// <param name="guests">The number of travellers.</param>
    /// <returns>The accommodation subtotal in ZAR.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="nights"/> or <paramref name="guests"/> is not positive.
    /// </exception>
    public decimal CalculateSubtotal(int nights, int guests)
    {
        if (nights <= 0)
            throw new ArgumentOutOfRangeException(nameof(nights), "A stay must be at least one night.");

        if (guests <= 0)
            throw new ArgumentOutOfRangeException(nameof(guests), "A stay must have at least one guest.");

        return PricePerNight * nights * guests;
    }

    /// <summary>
    /// Calculates the total price of a stay:
    /// <c>(PricePerNight × Nights × Guests) + CleaningFee + ServiceFee</c>.
    /// </summary>
    /// <param name="nights">The number of nights booked.</param>
    /// <param name="guests">The number of travellers. Defaults to one.</param>
    /// <returns>The total payable amount in ZAR, rounded to two decimals.</returns>
    public decimal CalculateTotalPrice(int nights, int guests = 1)
    {
        var subtotal = CalculateSubtotal(nights, guests);
        var serviceFee = Math.Round(subtotal * ServiceFeeRate, 2);
        return Math.Round(subtotal + CleaningFee + serviceFee, 2);
    }

    /// <summary>
    /// Calculates the service fee charged on a stay. The fee is a fraction of the
    /// accommodation subtotal, so it scales with party size too. The cleaning fee
    /// is excluded, being a flat per-stay charge.
    /// </summary>
    /// <param name="nights">The number of nights booked.</param>
    /// <param name="guests">The number of travellers. Defaults to one.</param>
    /// <returns>The service fee in ZAR.</returns>
    public decimal CalculateServiceFee(int nights, int guests = 1) =>
        Math.Round(CalculateSubtotal(nights, guests) * ServiceFeeRate, 2);
}
