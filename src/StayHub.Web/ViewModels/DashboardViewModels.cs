using StayHub.Core.Models;

namespace StayHub.Web.ViewModels;

/// <summary>
/// Backs the host dashboard: stat cards, quick actions, and listing cards.
/// </summary>
public class HostDashboardViewModel
{
    /// <summary>Gets or sets the signed-in host.</summary>
    public ApplicationUser Host { get; set; } = null!;

    /// <summary>Gets or sets the host's listings.</summary>
    public IEnumerable<HostProperty> Properties { get; set; } = new List<HostProperty>();

    /// <summary>Gets or sets the total number of listings.</summary>
    public int TotalProperties { get; set; }

    /// <summary>Gets or sets the number of published listings.</summary>
    public int ActiveProperties { get; set; }

    /// <summary>Gets or sets the total number of bookings received.</summary>
    public int TotalBookings { get; set; }

    /// <summary>Gets or sets the bookings received this calendar month.</summary>
    public int BookingsThisMonth { get; set; }

    /// <summary>Gets or sets the total earnings in ZAR.</summary>
    public decimal TotalEarnings { get; set; }

    /// <summary>Gets or sets the earnings for this calendar month in ZAR.</summary>
    public decimal EarningsThisMonth { get; set; }

    /// <summary>Gets or sets the average star rating across all listings.</summary>
    public double AverageRating { get; set; }

    /// <summary>Gets or sets the total number of reviews received.</summary>
    public int ReviewCount { get; set; }

    /// <summary>Gets or sets the number of booking requests awaiting a decision.</summary>
    public int PendingRequests { get; set; }
}

/// <summary>
/// Backs the admin dashboard: stat cards and the three recent-activity tables.
/// </summary>
public class AdminDashboardViewModel
{
    /// <summary>Gets or sets the number of registered users.</summary>
    public int TotalUsers { get; set; }

    /// <summary>Gets or sets the number of applications awaiting review.</summary>
    public int PendingApplications { get; set; }

    /// <summary>Gets or sets the number of published listings.</summary>
    public int ActiveProperties { get; set; }

    /// <summary>Gets or sets total platform revenue in ZAR.</summary>
    public decimal PlatformRevenue { get; set; }

    /// <summary>Gets or sets the total number of bookings.</summary>
    public int TotalBookings { get; set; }

    /// <summary>Gets or sets the number of bookings awaiting host approval.</summary>
    public int PendingBookings { get; set; }

    /// <summary>Gets or sets the number of approved hosts.</summary>
    public int ApprovedHosts { get; set; }

    /// <summary>Gets or sets the most recent audit-trail entries.</summary>
    public IEnumerable<ActivityLog> RecentActivity { get; set; } = new List<ActivityLog>();

    /// <summary>Gets or sets the most recent host applications.</summary>
    public IEnumerable<HostApplication> RecentApplications { get; set; } = new List<HostApplication>();

    /// <summary>Gets or sets the most recent bookings.</summary>
    public IEnumerable<Booking> RecentBookings { get; set; } = new List<Booking>();
}

/// <summary>
/// Backs the administrator's user-management table.
/// </summary>
public class UserManagementViewModel
{
    /// <summary>Gets or sets the users to render, each with their current roles.</summary>
    public IEnumerable<UserRow> Users { get; set; } = new List<UserRow>();

    /// <summary>Gets or sets the current search term.</summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the signed-in administrator may
    /// promote guests to Admin. Only a SuperAdmin can.
    /// </summary>
    public bool CanPromote { get; set; }

    /// <summary>
    /// One row of the user table.
    /// </summary>
    /// <param name="User">The user.</param>
    /// <param name="Roles">The roles the user currently holds.</param>
    public record UserRow(ApplicationUser User, IList<string> Roles);
}

/// <summary>
/// Backs the reusable statistic card partial.
/// </summary>
/// <param name="Label">Small uppercase heading, e.g. "Total Users".</param>
/// <param name="Value">The figure itself, pre-formatted for display.</param>
/// <param name="Caption">Supporting line beneath the figure.</param>
/// <param name="Icon">Icon name understood by the _Icon partial, e.g. "users".</param>
/// <param name="Tone">Optional icon tint: "sun", "clay", or "leaf". Blue when omitted.</param>
public record StatCard(
    string Label,
    string Value,
    string Caption,
    string Icon = "dashboard",
    string Tone = "");

/// <summary>
/// Backs the reusable empty-state partial shown when a collection has no rows.
/// </summary>
/// <param name="Image">Illustration file name in wwwroot/images/empty, without extension.</param>
/// <param name="Title">Short heading, e.g. "No bookings yet".</param>
/// <param name="Message">One sentence explaining what will fill this space.</param>
/// <param name="ActionText">Optional call to action label. No button is rendered when empty.</param>
/// <param name="ActionController">Controller the call to action links to.</param>
/// <param name="ActionAction">Action the call to action links to.</param>
public record EmptyState(
    string Image,
    string Title,
    string Message,
    string ActionText = "",
    string ActionController = "",
    string ActionAction = "");

/// <summary>
/// A single entry in a listing or table-row actions dropdown.
/// </summary>
/// <param name="Label">Text shown in the menu.</param>
/// <param name="Href">Link target. Use for navigation actions.</param>
/// <param name="Controller">Controller for form posts.</param>
/// <param name="Action">Action for form posts.</param>
/// <param name="HiddenFields">Hidden inputs submitted with form actions.</param>
/// <param name="ConfirmMessage">When set, the branded confirm dialog is shown before submit.</param>
/// <param name="ConfirmTitle">Optional confirm dialog title.</param>
/// <param name="ConfirmAction">Optional confirm button label.</param>
/// <param name="Danger">Styles the item as destructive.</param>
/// <param name="OpenInNewTab">Opens link actions in a new tab.</param>
public record ActionMenuItem(
    string Label,
    string? Href = null,
    string? Controller = null,
    string? Action = null,
    IReadOnlyDictionary<string, string>? HiddenFields = null,
    string? ConfirmMessage = null,
    string? ConfirmTitle = null,
    string? ConfirmAction = null,
    bool Danger = false,
    bool OpenInNewTab = false);
