namespace StayHub.Core.Models;

/// <summary>
/// The host-defined arrival procedure for one property. The individual steps are
/// stored as serialised JSON in <see cref="StepsJson"/> so a host can define any
/// number of them without a schema change.
/// </summary>
public class CheckInProcess
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the owning listing's identifier.</summary>
    public int PropertyId { get; set; }

    /// <summary>Gets or sets the owning listing.</summary>
    public HostProperty? Property { get; set; }

    /// <summary>Gets or sets the process title shown to the guest.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ordered steps as a serialised JSON array.
    /// </summary>
    public string StepsJson { get; set; } = "[]";

    /// <summary>Gets or sets the UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the guest check-ins driven by this process.</summary>
    public ICollection<GuestCheckIn> GuestCheckIns { get; set; } = new List<GuestCheckIn>();
}
