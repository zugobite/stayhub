using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StayHub.Core.Enums;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;
using StayHub.Web.ViewModels;

namespace StayHub.Web.Controllers;

/// <summary>
/// Serves the public property gallery and detail pages.
/// </summary>
public class PropertiesController : Controller
{
    private readonly IPropertyRepository _properties;
    private readonly IWishlistRepository _wishlist;
    private readonly IAmenityRepository _amenities;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initialises a new instance of the <see cref="PropertiesController"/> class.
    /// </summary>
    /// <param name="properties">The listing repository.</param>
    /// <param name="wishlist">The saved-listing repository.</param>
    /// <param name="amenities">The amenity lookup repository.</param>
    /// <param name="userManager">The Identity user manager.</param>
    public PropertiesController(
        IPropertyRepository properties,
        IWishlistRepository wishlist,
        IAmenityRepository amenities,
        UserManager<ApplicationUser> userManager)
    {
        _properties = properties;
        _wishlist = wishlist;
        _amenities = amenities;
        _userManager = userManager;
    }

    /// <summary>
    /// Renders the gallery of active listings, filtered by city and type.
    /// </summary>
    /// <param name="city">Optional city filter.</param>
    /// <param name="type">Optional listing-type filter.</param>
    /// <returns>The gallery page.</returns>
    [HttpGet]
    public async Task<IActionResult> Index(
        string? q = null,
        string? city = null,
        Province? province = null,
        PropertyType? type = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? guests = null,
        int? bedrooms = null,
        int? bathrooms = null,
        int[]? amenities = null,
        PropertySortOrder sort = PropertySortOrder.Newest)
    {
        // A blank text box arrives as an empty string; treat that as "no filter"
        // so it does not count towards HasAnyFilter and light up the clear button.
        var criteria = new PropertySearchCriteria
        {
            Keyword = string.IsNullOrWhiteSpace(q) ? null : q.Trim(),
            City = string.IsNullOrWhiteSpace(city) ? null : city,
            Province = province,
            PropertyType = type,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Guests = guests,
            Bedrooms = bedrooms,
            Bathrooms = bathrooms,
            AmenityIds = amenities ?? Array.Empty<int>(),
            SortOrder = sort
        };

        var (lowest, highest) = await _properties.GetPriceRangeAsync();

        var model = new PropertyGalleryViewModel
        {
            Properties = await _properties.SearchAsync(criteria),
            Cities = await _properties.GetCitiesAsync(),
            Provinces = await _properties.GetProvincesAsync(),
            Amenities = await _amenities.GetAllAsync(),
            Criteria = criteria,
            LowestPrice = lowest,
            HighestPrice = highest
        };

        var userId = _userManager.GetUserId(User);
        if (userId is not null)
        {
            var saved = await _wishlist.GetByUserAsync(userId);
            model.WishlistedIds = saved.Select(w => w.PropertyId).ToHashSet();
        }

        return View(model);
    }

    /// <summary>
    /// Renders a listing's detail page, photo gallery, amenities, and booking widget.
    /// </summary>
    /// <param name="id">The listing identifier.</param>
    /// <returns>The detail page, or 404 when the listing does not exist.</returns>
    [HttpGet]
    public async Task<IActionResult> View(int id)
    {
        var property = await _properties.GetByIdAsync(id);

        if (property is null)
            return NotFound();

        var model = new PropertyDetailViewModel
        {
            Property = property,
            ReviewCount = property.Reviews.Count,
            AverageRating = property.Reviews.Count == 0 ? 0 : property.Reviews.Average(r => r.Rating),
            BookingRequest = new BookingRequestViewModel { PropertyId = property.Id }
        };

        // Summarise the host so the guest can see who they would be staying with.
        if (!string.IsNullOrEmpty(property.HostId))
        {
            var hostListings = (await _properties.GetByHostAsync(property.HostId))
                .Where(p => p.IsActive)
                .ToList();

            var hostReviews = hostListings.SelectMany(p => p.Reviews).ToList();

            model.HostListingCount = hostListings.Count;
            model.HostReviewCount = hostReviews.Count;
            model.HostAverageRating = hostReviews.Count == 0 ? 0 : hostReviews.Average(r => r.Rating);
            model.MoreFromHost = hostListings.Where(p => p.Id != property.Id).Take(3).ToList();
        }

        var userId = _userManager.GetUserId(User);
        if (userId is not null)
        {
            var saved = await _wishlist.GetByUserAsync(userId);
            model.IsWishlisted = saved.Any(w => w.PropertyId == id);
        }

        return View(model);
    }
}
