using System.ComponentModel.DataAnnotations;

namespace StayHub.Web.ViewModels;

/// <summary>
/// Binding model for the footer newsletter signup form.
/// </summary>
public class NewsletterSubscribeViewModel
{
    /// <summary>Gets or sets the subscriber email address.</summary>
    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}
