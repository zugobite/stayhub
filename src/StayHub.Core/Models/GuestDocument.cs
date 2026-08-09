using StayHub.Core.Enums;

namespace StayHub.Core.Models;

/// <summary>
/// An identity document (ID or passport) a guest uploaded for host verification.
/// </summary>
public class GuestDocument
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the owning check-in's identifier.</summary>
    public int GuestCheckInId { get; set; }

    /// <summary>Gets or sets the owning check-in.</summary>
    public GuestCheckIn? GuestCheckIn { get; set; }

    /// <summary>Gets or sets the document kind, for example "Passport".</summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the stored file name.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the relative URL of the stored file.</summary>
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the verification state.</summary>
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

    /// <summary>Gets or sets the UTC upload timestamp.</summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
