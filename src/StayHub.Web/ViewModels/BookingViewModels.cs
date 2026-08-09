using System.ComponentModel.DataAnnotations;
using StayHub.Core.Models;

namespace StayHub.Web.ViewModels;

/// <summary>
/// Backs the booking widget on the property detail page.
/// </summary>
public class BookingRequestViewModel : IValidatableObject
{
    /// <summary>Gets or sets the listing being booked.</summary>
    public int PropertyId { get; set; }

    /// <summary>Gets or sets the arrival date.</summary>
    [Required, DataType(DataType.Date), Display(Name = "Check in")]
    public DateTime CheckInDate { get; set; } = DateTime.Today.AddDays(1);

    /// <summary>Gets or sets the departure date.</summary>
    [Required, DataType(DataType.Date), Display(Name = "Checkout")]
    public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(2);

    /// <summary>Gets or sets the number of travellers.</summary>
    [Range(1, 50), Display(Name = "Guests")]
    public int NumberOfGuests { get; set; } = 1;

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckOutDate <= CheckInDate)
        {
            yield return new ValidationResult(
                "The checkout date must fall after the check-in date.",
                new[] { nameof(CheckOutDate) });
        }

        if (CheckInDate.Date < DateTime.Today)
        {
            yield return new ValidationResult(
                "The check-in date cannot be in the past.",
                new[] { nameof(CheckInDate) });
        }
    }
}

/// <summary>
/// Backs the guest's trips list.
/// </summary>
public class MyTripsViewModel
{
    /// <summary>Gets or sets the guest's bookings.</summary>
    public IEnumerable<Booking> Bookings { get; set; } = new List<Booking>();
}
