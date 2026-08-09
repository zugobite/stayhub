using System.ComponentModel.DataAnnotations;

namespace StayHub.Core.Enums;

/// <summary>
/// The orderings offered by the property gallery's sort control.
/// </summary>
public enum PropertySortOrder
{
    /// <summary>Most recently published first. The default.</summary>
    [Display(Name = "Newest First")]
    Newest,

    /// <summary>Cheapest nightly rate first.</summary>
    [Display(Name = "Price: low to high")]
    PriceLowToHigh,

    /// <summary>Most expensive nightly rate first.</summary>
    [Display(Name = "Price: high to low")]
    PriceHighToLow,

    /// <summary>Highest average guest rating first.</summary>
    [Display(Name = "Top rated")]
    TopRated,

    /// <summary>Largest sleeping capacity first.</summary>
    [Display(Name = "Sleeps the most")]
    Capacity
}
