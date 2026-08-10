using System.ComponentModel.DataAnnotations;

namespace StayHub.Web.ViewModels;

/// <summary>
/// Backs the profile editor, where a user maintains the details other people see.
/// </summary>
public class ProfileViewModel
{
    /// <summary>Gets or sets the user's first name.</summary>
    [Required(ErrorMessage = "Enter your first name.")]
    [StringLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's surname.</summary>
    [Required(ErrorMessage = "Enter your surname.")]
    [StringLength(100)]
    [Display(Name = "Surname")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the introduction shown on listing pages. Hosts get the most
    /// from this, but any user may write one.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Keep your introduction under 1000 characters.")]
    [Display(Name = "About you")]
    public string? Bio { get; set; }

    /// <summary>Gets or sets the town or city shown beside the bio.</summary>
    [StringLength(120)]
    [Display(Name = "Where you are based")]
    public string? Location { get; set; }

    /// <summary>Gets or sets the stored photo URL, or <c>null</c> when none is set.</summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>Gets or sets a newly uploaded photo.</summary>
    [Display(Name = "Profile photo")]
    public IFormFile? Photo { get; set; }

    /// <summary>Gets or sets a value indicating whether the existing photo should be removed.</summary>
    [Display(Name = "Remove my photo")]
    public bool RemovePhoto { get; set; }

    /// <summary>Gets or sets the sign-in email, shown read-only.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the roles held, shown read-only.</summary>
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

    /// <summary>Gets or sets when the account was created.</summary>
    public DateTime MemberSince { get; set; }

    /// <summary>Gets the user's display name.</summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}
