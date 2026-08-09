using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using StayHub.Core.Constants;
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
    private readonly IReviewRepository _reviews;
    private readonly INewsletterRepository _newsletter;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initialises a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="properties">The listing repository.</param>
    /// <param name="reviews">The review repository, used for testimonials.</param>
    /// <param name="newsletter">The newsletter subscription repository.</param>
    /// <param name="userManager">The Identity user manager, used for the host count.</param>
    public HomeController(
        IPropertyRepository properties,
        IReviewRepository reviews,
        INewsletterRepository newsletter,
        UserManager<ApplicationUser> userManager)
    {
        _properties = properties;
        _reviews = reviews;
        _newsletter = newsletter;
        _userManager = userManager;
    }

    /// <summary>
    /// Renders the landing page: hero, trust bar, regional discovery, featured
    /// listings, how it works, guest testimonials, and the host proposition.
    /// </summary>
    /// <returns>The landing page.</returns>
    public async Task<IActionResult> Index()
    {
        var active = (await _properties.GetActiveAsync()).ToList();
        var provinceCounts = await _properties.GetProvinceCountsAsync();
        var (lowest, _) = await _properties.GetPriceRangeAsync();
        var hosts = await _userManager.GetUsersInRoleAsync(Roles.Host);

        var allReviews = active.SelectMany(p => p.Reviews).ToList();

        // One cover image per province, taken from that province's newest listing,
        // so the discovery cards stay in step with real inventory.
        var provinceImages = active
            .OrderByDescending(p => p.CreatedAt)
            .GroupBy(p => p.Province)
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(p => p.Images)
                      .OrderByDescending(i => i.IsCover)
                      .Select(i => i.ImageUrl)
                      .FirstOrDefault() ?? "/images/placeholder.svg");

        var model = new HomeViewModel
        {
            Featured = active
                .OrderByDescending(p => p.Reviews.Count)
                .ThenByDescending(p => p.CreatedAt)
                .Take(6)
                .ToList(),
            ListingCount = active.Count,
            ProvinceCount = provinceCounts.Count,
            HostCount = hosts.Count,
            ReviewCount = allReviews.Count,
            AverageRating = allReviews.Count == 0 ? 0 : allReviews.Average(r => r.Rating),
            LowestPrice = lowest,
            ProvinceCounts = provinceCounts,
            ProvinceImages = provinceImages,
            Testimonials = await _reviews.GetRecentHighlightsAsync(3)
        };

        return View(model);
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
    /// Renders the privacy policy.
    /// </summary>
    [HttpGet]
    public IActionResult Privacy() => View("Legal", new LegalPageViewModel
    {
        Title = "Privacy Policy",
        Intro = "How StayHub collects, uses, and protects personal information on the platform.",
        Sections = new[]
        {
            new LegalSection("Information we collect",
                "We collect account details you provide (name, email, phone), booking and hosting activity, messages you send on StayHub, and basic technical data such as browser type and approximate location needed to run the service."),
            new LegalSection("How we use information",
                "We use this information to operate bookings and payments simulation, verify hosts, deliver messages and notifications, improve the marketplace, and meet legal obligations under South African law where applicable."),
            new LegalSection("Sharing",
                "We share only what is needed for a stay or application: guests and hosts see each other’s relevant booking details, and administrators may review applications, bookings, and activity logs to keep the platform safe."),
            new LegalSection("Your choices",
                "You may update your profile details, unsubscribe from the newsletter, and request account-related corrections by contacting StayHub support through the platform administrators.")
        }
    });

    /// <summary>
    /// Renders the terms of service.
    /// </summary>
    [HttpGet]
    public IActionResult Terms() => View("Legal", new LegalPageViewModel
    {
        Title = "Terms of Service",
        Intro = "The rules for using StayHub as a guest, host, or administrator.",
        Sections = new[]
        {
            new LegalSection("Using StayHub",
                "StayHub is a marketplace for short stays in South Africa. You must provide accurate information, keep your login secure, and use messaging and bookings only for legitimate stay-related purposes."),
            new LegalSection("Bookings and listings",
                "Hosts are responsible for the accuracy of their listings. Guests are responsible for paying agreed amounts in the simulated checkout flow and following house rules. StayHub may cancel listings or bookings that breach these terms."),
            new LegalSection("Host applications",
                "Applying to host does not guarantee approval. Administrators may approve or reject applications and additional properties based on the information and documents you submit."),
            new LegalSection("Acceptable use",
                "Do not misuse the platform, attempt to bypass role restrictions, upload harmful content, or harass other users. StayHub may suspend accounts that violate these terms.")
        }
    });

    /// <summary>
    /// Renders the cookie policy.
    /// </summary>
    [HttpGet]
    public IActionResult Cookies() => View("Legal", new LegalPageViewModel
    {
        Title = "Cookie Policy",
        Intro = "How StayHub uses cookies and similar technologies in the browser.",
        Sections = new[]
        {
            new LegalSection("Essential cookies",
                "We use essential cookies and local storage to keep you signed in, protect forms with anti-forgery tokens, and remember session state required for secure browsing."),
            new LegalSection("Preferences",
                "Where supported, we may store light preferences such as UI state so the site behaves consistently across pages during your visit."),
            new LegalSection("Managing cookies",
                "You can clear or block cookies in your browser settings. Blocking essential cookies may prevent sign-in, messaging, or booking flows from working correctly.")
        }
    });

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
