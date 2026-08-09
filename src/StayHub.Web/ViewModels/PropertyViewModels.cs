using System.ComponentModel.DataAnnotations;
using StayHub.Core.Enums;
using StayHub.Core.Models;

namespace StayHub.Web.ViewModels;

/// <summary>
/// Backs the guest-facing gallery, including its city and type filters.
/// </summary>
public class PropertyGalleryViewModel
{
    /// <summary>Gets or sets the listings to render.</summary>
    public IEnumerable<HostProperty> Properties { get; set; } = new List<HostProperty>();

    /// <summary>Gets or sets the cities available in the filter dropdown.</summary>
    public IEnumerable<string> Cities { get; set; } = new List<string>();

    /// <summary>Gets or sets the provinces that currently have active listings.</summary>
    public IEnumerable<Province> Provinces { get; set; } = new List<Province>();

    /// <summary>Gets or sets the amenities offered by the amenity checkboxes.</summary>
    public IEnumerable<Amenity> Amenities { get; set; } = new List<Amenity>();

    /// <summary>Gets or sets the filters currently applied.</summary>
    public PropertySearchCriteria Criteria { get; set; } = new();

    /// <summary>Gets or sets the cheapest nightly rate on the platform, bounding the price filter.</summary>
    public decimal LowestPrice { get; set; }

    /// <summary>Gets or sets the dearest nightly rate on the platform, bounding the price filter.</summary>
    public decimal HighestPrice { get; set; }

    /// <summary>Gets or sets the listing identifiers the signed-in user has saved.</summary>
    public HashSet<int> WishlistedIds { get; set; } = new();

    /// <summary>Gets the selected city filter.</summary>
    public string? SelectedCity => Criteria.City;

    /// <summary>Gets the selected listing-type filter.</summary>
    public PropertyType? SelectedType => Criteria.PropertyType;
}

/// <summary>
/// Backs the property detail page and its sticky booking widget.
/// </summary>
public class PropertyDetailViewModel
{
    /// <summary>Gets or sets the listing being viewed.</summary>
    public HostProperty Property { get; set; } = null!;

    /// <summary>Gets or sets the listing's average star rating.</summary>
    public double AverageRating { get; set; }

    /// <summary>Gets or sets the number of reviews.</summary>
    public int ReviewCount { get; set; }

    /// <summary>Gets or sets a value indicating whether the signed-in user has saved this listing.</summary>
    public bool IsWishlisted { get; set; }

    /// <summary>Gets or sets the booking request being composed.</summary>
    public BookingRequestViewModel BookingRequest { get; set; } = new();

    /// <summary>Gets or sets the number of active listings the host runs.</summary>
    public int HostListingCount { get; set; }

    /// <summary>Gets or sets the number of reviews the host has received across all listings.</summary>
    public int HostReviewCount { get; set; }

    /// <summary>Gets or sets the host's average rating across all listings, or 0 when unreviewed.</summary>
    public double HostAverageRating { get; set; }

    /// <summary>Gets or sets other active listings by the same host, excluding this one.</summary>
    public IEnumerable<HostProperty> MoreFromHost { get; set; } = new List<HostProperty>();
}

/// <summary>
/// Backs the "Become a Host" application form. At least one photograph is
/// mandatory; the application cannot be submitted without it.
/// </summary>
public class HostApplicationViewModel
{
    /// <summary>Gets or sets the listing headline.</summary>
    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the listing description.</summary>
    [Required, StringLength(4000, MinimumLength = 20)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the nightly rate in ZAR.</summary>
    [Required, Range(1, 1_000_000), Display(Name = "Price per night (R)")]
    public decimal PricePerNight { get; set; }

    /// <summary>Gets or sets the one-off cleaning fee in ZAR.</summary>
    [Range(0, 100_000), Display(Name = "Cleaning fee (R)")]
    public decimal CleaningFee { get; set; }

    /// <summary>Gets or sets the street address.</summary>
    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;

    /// <summary>Gets or sets the city.</summary>
    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    /// <summary>Gets or sets the listing category.</summary>
    [Display(Name = "Property type")]
    public PropertyType PropertyType { get; set; }

    /// <summary>Gets or sets the maximum number of travellers.</summary>
    [Range(1, 50), Display(Name = "Maximum guests")]
    public int MaxGuests { get; set; } = 2;

    /// <summary>Gets or sets the bedroom count.</summary>
    [Range(0, 50)]
    public int Bedrooms { get; set; } = 1;

    /// <summary>Gets or sets the bed count.</summary>
    [Range(0, 50)]
    public int Beds { get; set; } = 1;

    /// <summary>Gets or sets the bathroom count.</summary>
    [Range(0, 50)]
    public int Bathrooms { get; set; } = 1;

    /// <summary>
    /// Gets or sets the uploaded photographs. Server-side validation rejects the
    /// application when this is empty.
    /// </summary>
    [Display(Name = "Property photos")]
    public List<IFormFile> Images { get; set; } = new();
}

/// <summary>
/// Backs the host's listing editor.
/// </summary>
public class EditPropertyViewModel : HostApplicationViewModel
{
    /// <summary>Gets or sets the listing identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the province.</summary>
    [Display(Name = "Province")]
    public Province Province { get; set; }

    /// <summary>Gets or sets a value indicating whether the listing is published.</summary>
    [Display(Name = "Listing is active")]
    public bool IsActive { get; set; }

    /// <summary>Gets or sets the photographs already attached to the listing.</summary>
    public IEnumerable<PropertyImage> ExistingImages { get; set; } = new List<PropertyImage>();
}
