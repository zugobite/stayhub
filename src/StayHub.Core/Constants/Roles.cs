namespace StayHub.Core.Constants;

/// <summary>
/// Canonical role names used by ASP.NET Core Identity and every
/// <c>[Authorize(Roles = ...)]</c> attribute in the application.
/// </summary>
public static class Roles
{
    /// <summary>
    /// Seeded super administrator. The only role permitted to promote a Guest to Admin.
    /// </summary>
    public const string SuperAdmin = "SuperAdmin";

    /// <summary>
    /// Platform administrator. Reviews host applications and monitors the activity log.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// An approved property owner. A user is either a Host or a Guest, never both.
    /// </summary>
    public const string Host = "Host";

    /// <summary>
    /// Default role assigned to every new registrant.
    /// </summary>
    public const string Guest = "Guest";

    /// <summary>
    /// Gets every role name that must exist in the database on startup.
    /// </summary>
    public static readonly string[] All = { SuperAdmin, Admin, Host, Guest };
}
