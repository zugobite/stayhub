using StayHub.Core.Enums;

namespace StayHub.Core.Models;

/// <summary>
/// A guest's reservation of a property. Status changes are encapsulated behind
/// transition methods so an invalid state change cannot be persisted.
/// </summary>
public class Booking
{
    private BookingStatus _status = BookingStatus.Pending;

    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the booked property's identifier.</summary>
    public int PropertyId { get; set; }

    /// <summary>Gets or sets the booked property.</summary>
    public HostProperty? Property { get; set; }

    /// <summary>Gets or sets the booking guest's identifier.</summary>
    public string GuestId { get; set; } = string.Empty;

    /// <summary>Gets or sets the booking guest.</summary>
    public ApplicationUser? Guest { get; set; }

    /// <summary>Gets or sets the arrival date.</summary>
    public DateTime CheckInDate { get; set; }

    /// <summary>Gets or sets the departure date.</summary>
    public DateTime CheckOutDate { get; set; }

    /// <summary>Gets or sets the number of travellers.</summary>
    public int NumberOfGuests { get; set; } = 1;

    /// <summary>Gets or sets the total payable amount in ZAR.</summary>
    public decimal TotalPrice { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the request was raised.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the current booking status. Change it through the transition methods.
    /// </summary>
    public BookingStatus Status
    {
        get => _status;
        set => _status = value; // Setter retained for EF Core materialisation.
    }

    /// <summary>Gets or sets the payment raised for this booking.</summary>
    public Payment? Payment { get; set; }

    /// <summary>Gets or sets the guest check-in raised for this booking.</summary>
    public GuestCheckIn? GuestCheckIn { get; set; }

    /// <summary>
    /// Gets the number of nights covered by the reservation.
    /// </summary>
    public int Nights => Math.Max((CheckOutDate.Date - CheckInDate.Date).Days, 0);

    /// <summary>
    /// Host approves a pending request.
    /// </summary>
    /// <returns><c>true</c> when the transition was valid; otherwise <c>false</c>.</returns>
    public bool Approve()
    {
        if (_status != BookingStatus.Pending)
            return false;

        _status = BookingStatus.Approved;
        return true;
    }

    /// <summary>
    /// Host rejects a pending request.
    /// </summary>
    /// <returns><c>true</c> when the transition was valid; otherwise <c>false</c>.</returns>
    public bool Reject()
    {
        if (_status != BookingStatus.Pending)
            return false;

        _status = BookingStatus.Rejected;
        return true;
    }

    /// <summary>
    /// Records settlement of an approved booking.
    /// </summary>
    /// <returns><c>true</c> when the transition was valid; otherwise <c>false</c>.</returns>
    public bool MarkPaid()
    {
        if (_status != BookingStatus.Approved)
            return false;

        _status = BookingStatus.Paid;
        return true;
    }

    /// <summary>
    /// Completes check-in once the host has verified the guest's documents.
    /// </summary>
    /// <returns><c>true</c> when the transition was valid; otherwise <c>false</c>.</returns>
    public bool CheckIn()
    {
        if (_status != BookingStatus.Approved && _status != BookingStatus.Paid)
            return false;

        _status = BookingStatus.CheckedIn;
        return true;
    }

    /// <summary>
    /// Closes the stay.
    /// </summary>
    /// <returns><c>true</c> when the transition was valid; otherwise <c>false</c>.</returns>
    public bool Complete()
    {
        if (_status != BookingStatus.CheckedIn)
            return false;

        _status = BookingStatus.Completed;
        return true;
    }

    /// <summary>
    /// Cancels a booking that has not yet been checked in.
    /// </summary>
    /// <returns><c>true</c> when the transition was valid; otherwise <c>false</c>.</returns>
    public bool Cancel()
    {
        if (_status is BookingStatus.CheckedIn or BookingStatus.Completed or BookingStatus.Cancelled)
            return false;

        _status = BookingStatus.Cancelled;
        return true;
    }
}
