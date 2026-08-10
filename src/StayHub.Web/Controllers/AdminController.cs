using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StayHub.Core.Constants;
using StayHub.Core.Enums;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;
using StayHub.Web.Services;
using StayHub.Web.ViewModels;

namespace StayHub.Web.Controllers;

/// <summary>
/// The administrative command centre: application review, user management, and
/// the system audit trail. Promotion to Admin is restricted to the SuperAdmin.
/// </summary>
[Authorize(Roles = $"{Roles.Admin},{Roles.SuperAdmin}")]
public class AdminController : Controller
{
    private readonly IHostApplicationRepository _applications;
    private readonly IPropertyRepository _properties;
    private readonly IBookingRepository _bookings;
    private readonly IActivityLogRepository _activityLog;
    private readonly INotificationRepository _notifications;
    private readonly IUserRepository _users;
    private readonly IRoleTransitionService _roleTransitions;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initialises a new instance of the <see cref="AdminController"/> class.
    /// </summary>
    /// <param name="applications">The application repository.</param>
    /// <param name="properties">The listing repository.</param>
    /// <param name="bookings">The booking repository.</param>
    /// <param name="activityLog">The audit trail repository.</param>
    /// <param name="notifications">The notification repository.</param>
    /// <param name="users">The user repository.</param>
    /// <param name="roleTransitions">The role transition service.</param>
    /// <param name="userManager">The Identity user manager.</param>
    public AdminController(
        IHostApplicationRepository applications,
        IPropertyRepository properties,
        IBookingRepository bookings,
        IActivityLogRepository activityLog,
        INotificationRepository notifications,
        IUserRepository users,
        IRoleTransitionService roleTransitions,
        UserManager<ApplicationUser> userManager)
    {
        _applications = applications;
        _properties = properties;
        _bookings = bookings;
        _activityLog = activityLog;
        _notifications = notifications;
        _users = users;
        _roleTransitions = roleTransitions;
        _userManager = userManager;
    }

    /// <summary>
    /// Renders the admin dashboard: stat cards plus the recent activity,
    /// application, and booking tables.
    /// </summary>
    /// <returns>The dashboard page.</returns>
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var applications = (await _applications.GetAllAsync()).ToList();
        var activeProperties = (await _properties.GetActiveAsync()).ToList();
        var hosts = await _userManager.GetUsersInRoleAsync(Roles.Host);

        var allBookings = new List<Booking>();
        foreach (var host in hosts)
            allBookings.AddRange(await _bookings.GetByHostAsync(host.Id));

        var earning = new[] { BookingStatus.Paid, BookingStatus.CheckedIn, BookingStatus.Completed };

        var model = new AdminDashboardViewModel
        {
            TotalUsers = await _users.CountAsync(),
            PendingApplications = applications.Count(a => a.Status == ApplicationStatus.Pending),
            ActiveProperties = activeProperties.Count,
            PlatformRevenue = allBookings.Where(b => earning.Contains(b.Status)).Sum(b => b.TotalPrice),
            TotalBookings = allBookings.Count,
            PendingBookings = allBookings.Count(b => b.Status == BookingStatus.Pending),
            ApprovedHosts = hosts.Count,
            RecentActivity = await _activityLog.GetRecentAsync(10),
            RecentApplications = applications.Take(5).ToList(),
            RecentBookings = allBookings.OrderByDescending(b => b.CreatedAt).Take(5).ToList()
        };

        return View(model);
    }

    /// <summary>
    /// Renders the user table, with promotion controls when the signed-in
    /// administrator is the SuperAdmin.
    /// </summary>
    /// <param name="search">Optional name or email search term.</param>
    /// <returns>The user management page.</returns>
    [HttpGet]
    public async Task<IActionResult> Users(string? search = null, int page = 1)
    {
        var users = string.IsNullOrWhiteSpace(search)
            ? await _users.GetAllAsync()
            : await _users.SearchAsync(search);

        var rows = new List<UserManagementViewModel.UserRow>();

        foreach (var user in users)
            rows.Add(new UserManagementViewModel.UserRow(user, await _userManager.GetRolesAsync(user)));

        return View(new UserManagementViewModel
        {
            Users = PagedList<UserManagementViewModel.UserRow>.Create(rows, page),
            SearchTerm = search,
            CanPromote = User.IsInRole(Roles.SuperAdmin)
        });
    }

    /// <summary>
    /// Promotes a Guest to Admin. Restricted to the SuperAdmin.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>A redirect to the user table.</returns>
    [HttpPost]
    [Authorize(Roles = Roles.SuperAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteToAdmin(string id)
    {
        var user = await _users.GetByIdAsync(id);

        if (user is null)
            return NotFound();

        var actingUserId = _userManager.GetUserId(User)!;

        if (!await _roleTransitions.PromoteToAdminAsync(user, actingUserId))
        {
            TempData["Error"] = "The promotion could not be applied.";
            return RedirectToAction(nameof(Users));
        }

        await _notifications.CreateAsync(
            user.Id,
            "You are now an administrator",
            "A SuperAdmin has granted you the Admin role on StayHub.",
            NotificationType.System,
            "/Admin/Dashboard");

        TempData["Success"] = $"{user.FullName} is now an administrator.";
        return RedirectToAction(nameof(Users));
    }

    /// <summary>
    /// Renders the host application review queue.
    /// </summary>
    /// <param name="status">Optional status filter.</param>
    /// <returns>The review queue page.</returns>
    [HttpGet]
    public async Task<IActionResult> HostRequests(ApplicationStatus? status = ApplicationStatus.Pending, int page = 1)
    {
        ViewBag.Status = status;
        var applications = await _applications.GetAllAsync(status);
        return View(PagedList<HostApplication>.Create(applications, page));
    }

    /// <summary>
    /// Renders one application in full, with its proposed listing and photographs.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <returns>The application detail page, or 404 when it does not exist.</returns>
    [HttpGet]
    public async Task<IActionResult> ReviewApplication(int id)
    {
        var application = await _applications.GetByIdAsync(id);
        return application is null ? NotFound() : View(application);
    }

    /// <summary>
    /// Approves an application: the listing is published, the applicant gains the
    /// Host role, and their Guest role is removed.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="notes">Optional reviewer note.</param>
    /// <returns>A redirect to the review queue.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveApplication(int id, string? notes = null) =>
        await DecideApplicationAsync(id, approve: true, notes);

    /// <summary>
    /// Rejects an application. The proposed listing stays unpublished.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="notes">Optional reviewer note.</param>
    /// <returns>A redirect to the review queue.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectApplication(int id, string? notes = null) =>
        await DecideApplicationAsync(id, approve: false, notes);

    /// <summary>
    /// Lists every booking on the platform.
    /// </summary>
    /// <returns>The bookings page.</returns>
    [HttpGet]
    public async Task<IActionResult> Bookings(int page = 1)
    {
        var hosts = await _userManager.GetUsersInRoleAsync(Roles.Host);
        var all = new List<Booking>();

        foreach (var host in hosts)
            all.AddRange(await _bookings.GetByHostAsync(host.Id));

        var ordered = all.OrderByDescending(b => b.CreatedAt).ToList();
        return View(PagedList<Booking>.Create(ordered, page));
    }

    /// <summary>
    /// Lists every published listing.
    /// </summary>
    /// <returns>The properties page.</returns>
    [HttpGet]
    public async Task<IActionResult> Properties() => View(await _properties.GetActiveAsync());

    /// <summary>
    /// Renders the system audit trail.
    /// </summary>
    /// <param name="type">Optional activity-type filter.</param>
    /// <returns>The activity log page.</returns>
    [HttpGet]
    public async Task<IActionResult> ActivityLogs(ActivityType? type = null, int page = 1)
    {
        ViewBag.SelectedType = type;

        // Paged in SQL rather than in memory: the audit trail grows with every
        // login and status change, so it must never be loaded whole.
        return View(await _activityLog.GetPagedAsync(page, PagedList<ActivityLog>.DefaultPageSize, type));
    }

    /// <summary>
    /// Applies an approve or reject decision to an application, performing the
    /// role swap and raising the applicant's notification.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="approve">Whether the administrator approved it.</param>
    /// <param name="notes">Optional reviewer note.</param>
    /// <returns>A redirect to the review queue.</returns>
    private async Task<IActionResult> DecideApplicationAsync(int id, bool approve, string? notes)
    {
        var application = await _applications.GetByIdAsync(id);

        if (application is null)
            return NotFound();

        if (application.Status != ApplicationStatus.Pending)
        {
            TempData["Error"] = "That application has already been reviewed.";
            return RedirectToAction(nameof(HostRequests));
        }

        var actingUserId = _userManager.GetUserId(User)!;

        application.Status = approve ? ApplicationStatus.Approved : ApplicationStatus.Rejected;
        application.ReviewNotes = notes;
        application.ReviewedAt = DateTime.UtcNow;
        await _applications.UpdateAsync(application);

        if (approve)
        {
            var applicant = await _users.GetByIdAsync(application.ApplicationUserId);

            if (applicant is not null)
                await _roleTransitions.PromoteGuestToHostAsync(applicant, actingUserId);

            var property = await _properties.GetByIdAsync(application.PropertyId);

            if (property is not null)
            {
                property.Activate();
                await _properties.UpdateAsync(property);
            }
        }

        await _activityLog.LogAsync(
            actingUserId,
            approve ? "HostApplicationApproved" : "HostApplicationRejected",
            ActivityType.ApplicationReviewed,
            $"Application #{application.Id} for '{application.Property?.Title}'.");

        await _notifications.CreateAsync(
            application.ApplicationUserId,
            approve ? "Host Application Approved" : "Host Application Rejected",
            approve
                ? $"Congratulations! Your host application for '{application.Property?.Title}' has been approved."
                : $"Your application for '{application.Property?.Title}' was not approved. {notes}".Trim(),
            approve ? NotificationType.HostApplicationApproved : NotificationType.System,
            approve ? "/Host/Dashboard" : "/HostApplications/Status");

        TempData["Success"] = approve ? "Application approved." : "Application rejected.";
        return RedirectToAction(nameof(HostRequests));
    }
}
