using System.ComponentModel.DataAnnotations;
using StayHub.Core.Models;

namespace StayHub.Web.ViewModels;

/// <summary>
/// Backs the leave-a-review form after a completed stay.
/// </summary>
public class LeaveReviewViewModel
{
    /// <summary>Gets or sets the booking that qualifies the guest to review.</summary>
    public int BookingId { get; set; }

    /// <summary>Gets or sets the listing being reviewed.</summary>
    public int PropertyId { get; set; }

    /// <summary>Gets or sets the listing title shown on the form.</summary>
    public string PropertyTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the star rating from 1 to 5.</summary>
    [Required, Range(1, 5), Display(Name = "Rating")]
    public int Rating { get; set; } = 5;

    /// <summary>Gets or sets the written feedback.</summary>
    [Required, StringLength(2000, MinimumLength = 10), Display(Name = "Your review")]
    public string Comment { get; set; } = string.Empty;
}
