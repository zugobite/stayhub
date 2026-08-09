using StayHub.Core.Enums;

namespace StayHub.Core.Models;

/// <summary>
/// One guest's run through a property's <see cref="CheckInProcess"/>.
/// </summary>
public class GuestCheckIn
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the associated booking's identifier.</summary>
    public int BookingId { get; set; }

    /// <summary>Gets or sets the associated booking.</summary>
    public Booking? Booking { get; set; }

    /// <summary>Gets or sets the driving check-in process's identifier.</summary>
    public int CheckInProcessId { get; set; }

    /// <summary>Gets or sets the driving check-in process.</summary>
    public CheckInProcess? CheckInProcess { get; set; }

    /// <summary>Gets or sets the index of the step the guest is currently on.</summary>
    public int CurrentStep { get; set; }

    /// <summary>Gets or sets the workflow state.</summary>
    public CheckInStatus Status { get; set; } = CheckInStatus.NotStarted;

    /// <summary>Gets or sets the UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the identity documents uploaded during this check-in.</summary>
    public ICollection<GuestDocument> Documents { get; set; } = new List<GuestDocument>();
}
