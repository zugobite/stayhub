using StayHub.Core.Enums;

namespace StayHub.Core.Models;

/// <summary>
/// A request to list a property. The first approved application promotes the
/// applicant from Guest to Host; later ones are additional-property applications.
/// </summary>
public class HostApplication
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the applicant's identifier.</summary>
    public string ApplicationUserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the applicant.</summary>
    public ApplicationUser? ApplicationUser { get; set; }

    /// <summary>Gets or sets the proposed listing's identifier.</summary>
    public int PropertyId { get; set; }

    /// <summary>Gets or sets the proposed listing.</summary>
    public HostProperty? Property { get; set; }

    /// <summary>Gets or sets the review state.</summary>
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    /// <summary>
    /// Gets or sets a value indicating whether the applicant was already a host,
    /// which distinguishes an additional-property application from the initial one.
    /// </summary>
    public bool IsAdditionalProperty { get; set; }

    /// <summary>Gets or sets the reviewing administrator's note.</summary>
    public string? ReviewNotes { get; set; }

    /// <summary>Gets or sets the UTC submission timestamp.</summary>
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the UTC timestamp at which an administrator ruled on it.</summary>
    public DateTime? ReviewedAt { get; set; }
}
