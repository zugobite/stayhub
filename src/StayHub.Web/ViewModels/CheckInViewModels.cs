using System.ComponentModel.DataAnnotations;
using StayHub.Core.Models;

namespace StayHub.Web.ViewModels;

/// <summary>
/// Backs the host's check-in process builder. The steps are serialised into
/// <see cref="CheckInProcess.StepsJson"/> on save.
/// </summary>
public class CheckInProcessViewModel
{
    /// <summary>Gets or sets the listing the process belongs to.</summary>
    public int PropertyId { get; set; }

    /// <summary>Gets or sets the listing title, shown as page context.</summary>
    public string PropertyTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the process title shown to guests.</summary>
    [Required, StringLength(200)]
    public string Title { get; set; } = "Arrival instructions";

    /// <summary>
    /// Gets or sets the ordered steps, one per line in the textarea.
    /// </summary>
    [Display(Name = "Steps (one per line)")]
    public string StepsText { get; set; } = string.Empty;

    /// <summary>Gets or sets the document type the guest must upload.</summary>
    [Display(Name = "Required document")]
    public string RequiredDocument { get; set; } = "ID or Passport";
}

/// <summary>
/// Backs the guest's multi-step check-in wizard.
/// </summary>
public class GuestCheckInViewModel
{
    /// <summary>Gets or sets the booking being checked into.</summary>
    public Booking Booking { get; set; } = null!;

    /// <summary>Gets or sets the check-in record.</summary>
    public GuestCheckIn CheckIn { get; set; } = null!;

    /// <summary>Gets or sets the steps the host defined.</summary>
    public IReadOnlyList<string> Steps { get; set; } = Array.Empty<string>();

    /// <summary>Gets or sets the zero-based index of the step being shown.</summary>
    public int CurrentStep { get; set; }

    /// <summary>Gets or sets the identity document type being uploaded.</summary>
    [Display(Name = "Document type")]
    public string DocumentType { get; set; } = "ID";

    /// <summary>Gets or sets the uploaded identity document.</summary>
    [Display(Name = "Upload ID or passport")]
    public IFormFile? Document { get; set; }

    /// <summary>Gets a value indicating whether the guest is on the final, upload step.</summary>
    public bool IsUploadStep => CurrentStep >= Steps.Count;
}
