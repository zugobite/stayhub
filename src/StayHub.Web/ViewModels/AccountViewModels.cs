using System.ComponentModel.DataAnnotations;

namespace StayHub.Web.ViewModels;

/// <summary>
/// Backs the registration form. Every registrant is assigned the Guest role.
/// </summary>
public class RegisterViewModel
{
    /// <summary>Gets or sets the registrant's first name.</summary>
    [Required, StringLength(100), Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Gets or sets the registrant's surname.</summary>
    [Required, StringLength(100), Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>Gets or sets the email address, which doubles as the username.</summary>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the chosen password.</summary>
    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets the password confirmation.</summary>
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Backs the sign-in form.
/// </summary>
public class LoginViewModel
{
    /// <summary>Gets or sets the account email address.</summary>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the account password.</summary>
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the session should persist.</summary>
    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    /// <summary>Gets or sets the URL to return to after a successful sign-in.</summary>
    public string? ReturnUrl { get; set; }
}
