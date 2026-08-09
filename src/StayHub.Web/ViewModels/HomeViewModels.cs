using StayHub.Core.Enums;
using StayHub.Core.Models;

namespace StayHub.Web.ViewModels;

/// <summary>
/// Backs the landing page.
/// </summary>
/// <remarks>
/// The section order follows conventional conversion research: value proposition
/// and call to action above the fold, trust signals immediately beneath, then
/// discovery, benefits, supporting evidence, and a repeated call to action.
/// Because StayHub is a two-sided marketplace, the guest and host propositions
/// are addressed in separate bands rather than blended into one message.
///
/// Every figure on this page is read from the database. Nothing is hard-coded,
/// so an empty platform reports an empty platform rather than inventing traction.
/// </remarks>
public class HomeViewModel
{
    /// <summary>Gets or sets the listings shown in the featured strip.</summary>
    public IEnumerable<HostProperty> Featured { get; set; } = new List<HostProperty>();

    /// <summary>Gets or sets the number of published listings.</summary>
    public int ListingCount { get; set; }

    /// <summary>Gets or sets the number of provinces with at least one listing.</summary>
    public int ProvinceCount { get; set; }

    /// <summary>Gets or sets the number of approved hosts.</summary>
    public int HostCount { get; set; }

    /// <summary>Gets or sets the total number of guest reviews.</summary>
    public int ReviewCount { get; set; }

    /// <summary>Gets or sets the mean guest rating across every review.</summary>
    public double AverageRating { get; set; }

    /// <summary>Gets or sets the cheapest nightly rate currently listed.</summary>
    public decimal LowestPrice { get; set; }

    /// <summary>Gets or sets the listing count per province, for the discovery cards.</summary>
    public IReadOnlyDictionary<Province, int> ProvinceCounts { get; set; }
        = new Dictionary<Province, int>();

    /// <summary>Gets or sets a representative cover image per province.</summary>
    public IReadOnlyDictionary<Province, string> ProvinceImages { get; set; }
        = new Dictionary<Province, string>();

    /// <summary>Gets or sets recent highly-rated reviews used as testimonials.</summary>
    public IEnumerable<Review> Testimonials { get; set; } = new List<Review>();

    /// <summary>
    /// Gets a value indicating whether the platform has enough data for the
    /// trust bar to say anything meaningful.
    /// </summary>
    public bool HasTraction => ListingCount > 0;
}
