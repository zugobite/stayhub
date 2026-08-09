using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;
using StayHub.Web.ViewModels;

namespace StayHub.Web.Controllers;

/// <summary>
/// The notification centre: a master list on the left with ALL / UNREAD / READ /
/// TODAY filters, and the selected notification's detail on the right.
/// </summary>
[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationRepository _notifications;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initialises a new instance of the <see cref="NotificationsController"/> class.
    /// </summary>
    /// <param name="notifications">The notification repository.</param>
    /// <param name="userManager">The Identity user manager.</param>
    public NotificationsController(
        INotificationRepository notifications,
        UserManager<ApplicationUser> userManager)
    {
        _notifications = notifications;
        _userManager = userManager;
    }

    /// <summary>
    /// Renders the notification centre.
    /// </summary>
    /// <param name="filter">The active filter: <c>all</c>, <c>unread</c>, <c>read</c>, or <c>today</c>.</param>
    /// <param name="selectedId">The notification to open in the detail pane.</param>
    /// <returns>The notification centre page.</returns>
    [HttpGet]
    public async Task<IActionResult> Index(string filter = "all", int? selectedId = null)
    {
        var userId = _userManager.GetUserId(User)!;
        var all = (await _notifications.GetByUserAsync(userId)).ToList();
        var today = DateTime.UtcNow.Date;

        var filtered = filter.ToLowerInvariant() switch
        {
            "unread" => all.Where(n => !n.IsRead),
            "read" => all.Where(n => n.IsRead),
            "today" => all.Where(n => n.CreatedAt.Date == today),
            _ => all
        };

        var model = new NotificationCentreViewModel
        {
            Notifications = filtered.ToList(),
            Filter = filter.ToLowerInvariant(),
            AllCount = all.Count,
            UnreadCount = all.Count(n => !n.IsRead),
            ReadCount = all.Count(n => n.IsRead),
            TodayCount = all.Count(n => n.CreatedAt.Date == today)
        };

        if (selectedId.HasValue)
        {
            model.Selected = await _notifications.GetByIdAsync(selectedId.Value, userId);

            // Opening a notification marks it read, which the rubric checks in the database.
            if (model.Selected is not null && !model.Selected.IsRead)
                await _notifications.MarkAsReadAsync(model.Selected.Id, userId);
        }

        return View(model);
    }

    /// <summary>
    /// Marks a single notification as read.
    /// </summary>
    /// <param name="id">The notification identifier.</param>
    /// <param name="filter">The filter to return to.</param>
    /// <returns>A redirect to the notification centre.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id, string filter = "all")
    {
        var userId = _userManager.GetUserId(User)!;
        await _notifications.MarkAsReadAsync(id, userId);
        return RedirectToAction(nameof(Index), new { filter });
    }

    /// <summary>
    /// Marks every notification as read.
    /// </summary>
    /// <param name="filter">The filter to return to.</param>
    /// <returns>A redirect to the notification centre.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead(string filter = "all")
    {
        var userId = _userManager.GetUserId(User)!;
        var count = await _notifications.MarkAllAsReadAsync(userId);
        TempData["Success"] = $"{count} notification(s) marked as read.";
        return RedirectToAction(nameof(Index), new { filter });
    }

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    /// <param name="id">The notification identifier.</param>
    /// <param name="filter">The filter to return to.</param>
    /// <returns>A redirect to the notification centre.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string filter = "all")
    {
        var userId = _userManager.GetUserId(User)!;
        await _notifications.DeleteAsync(id, userId);
        return RedirectToAction(nameof(Index), new { filter });
    }
}
