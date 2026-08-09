using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;
using StayHub.Web.Models;
using StayHub.Web.ViewModels;

namespace StayHub.Web.Controllers;

/// <summary>
/// Serves the public landing page, newsletter signup, and the shared error view.
/// </summary>
public class HomeController : Controller
{
    private readonly IPropertyRepository _properties;
    private readonly INewsletterRepository _newsletter;

    /// <summary>
    /// Initialises a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="properties">The listing repository.</param>
    /// <param name="newsletter">The newsletter subscription repository.</param>
    public HomeController(IPropertyRepository properties, INewsletterRepository newsletter)
    {
        _properties = properties;
        _newsletter = newsletter;
    }

    /// <summary>
    /// Renders the hero banner and a strip of featured active listings.
    /// </summary>
    /// <returns>The landing page.</returns>
    public async Task<IActionResult> Index()
    {
        var featured = (await _properties.GetActiveAsync()).Take(6);
        return View(featured);
    }

    /// <summary>
    /// Accepts a footer newsletter signup and redirects back to the referring page.
    /// </summary>
    /// <param name="model">The submitted email address.</param>
    /// <param name="returnUrl">Optional local URL to redirect to after signup.</param>
    /// <returns>A redirect with a success or error flash message.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(NewsletterSubscribeViewModel model, string? returnUrl = null)
    {
        IActionResult RedirectBack()
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Enter a valid email address.";
            return RedirectBack();
        }

        if (await _newsletter.ExistsAsync(model.Email))
        {
            TempData["Success"] = "You are already on the StayHub newsletter list.";
            return RedirectBack();
        }

        await _newsletter.AddAsync(new NewsletterSubscription
        {
            Email = model.Email,
            SubscribedAt = DateTime.UtcNow
        });

        TempData["Success"] = "Thanks - you are subscribed to the StayHub newsletter.";
        return RedirectBack();
    }

    /// <summary>
    /// Renders the shared error page.
    /// </summary>
    /// <returns>The error page.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
