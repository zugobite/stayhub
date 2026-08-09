using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Web.Controllers;

/// <summary>
/// Backs the wishlist heart on the gallery cards and the saved-listings page.
/// </summary>
[Authorize]
public class WishlistController : Controller
{
    private readonly IWishlistRepository _wishlist;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initialises a new instance of the <see cref="WishlistController"/> class.
    /// </summary>
    /// <param name="wishlist">The saved-listing repository.</param>
    /// <param name="userManager">The Identity user manager.</param>
    public WishlistController(IWishlistRepository wishlist, UserManager<ApplicationUser> userManager)
    {
        _wishlist = wishlist;
        _userManager = userManager;
    }

    /// <summary>
    /// Lists the signed-in user's saved listings.
    /// </summary>
    /// <returns>The wishlist page.</returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        return View(await _wishlist.GetByUserAsync(userId));
    }

    /// <summary>
    /// Adds or removes a listing from the wishlist.
    /// </summary>
    /// <param name="propertyId">The listing identifier.</param>
    /// <param name="returnUrl">The page to return to.</param>
    /// <returns>A redirect back to the originating page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int propertyId, string? returnUrl = null)
    {
        var userId = _userManager.GetUserId(User)!;
        var saved = await _wishlist.ToggleAsync(userId, propertyId);

        TempData["Success"] = saved ? "Added to your wishlist." : "Removed from your wishlist.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(PropertiesController.Index), "Properties");
    }
}
