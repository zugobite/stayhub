using StayHub.Core.Models;

namespace StayHub.Web.ViewModels;

/// <summary>
/// The non-generic view of a <see cref="PagedList{T}"/>, so one partial can render
/// the pager for every table regardless of the row type.
/// </summary>
/// <param name="PageNumber">The current one-based page.</param>
/// <param name="TotalPages">The number of pages available.</param>
/// <param name="TotalCount">The total row count across all pages.</param>
/// <param name="FirstItemNumber">The one-based index of the first row shown.</param>
/// <param name="LastItemNumber">The one-based index of the last row shown.</param>
/// <param name="ItemLabel">Plural noun for the rows, used in "Showing 1–15 of 42 bookings".</param>
public record PagerInfo(
    int PageNumber,
    int TotalPages,
    int TotalCount,
    int FirstItemNumber,
    int LastItemNumber,
    string ItemLabel)
{
    /// <summary>Gets a value indicating whether a previous page exists.</summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>Gets a value indicating whether a next page exists.</summary>
    public bool HasNext => PageNumber < TotalPages;

    /// <summary>Gets a value indicating whether the pager needs rendering.</summary>
    public bool HasMultiplePages => TotalPages > 1;

    /// <summary>
    /// Gets the page numbers to render, windowed around the current page so a long
    /// table does not produce an unusable strip of links.
    /// </summary>
    /// <param name="window">How many numbers to show at most.</param>
    /// <returns>The page numbers, in ascending order.</returns>
    public IEnumerable<int> PageWindow(int window = 7)
    {
        if (TotalPages <= window)
            return Enumerable.Range(1, TotalPages);

        var half = window / 2;
        var start = Math.Max(1, PageNumber - half);
        var end = Math.Min(TotalPages, start + window - 1);

        // Pull the window back when it would run past the last page.
        start = Math.Max(1, end - window + 1);

        return Enumerable.Range(start, end - start + 1);
    }
}

/// <summary>
/// Bridges <see cref="PagedList{T}"/> to the non-generic <see cref="PagerInfo"/>.
/// </summary>
public static class PagedListExtensions
{
    /// <summary>
    /// Describes the page for the pagination partial.
    /// </summary>
    /// <typeparam name="T">The row type.</typeparam>
    /// <param name="page">The page being rendered.</param>
    /// <param name="itemLabel">Plural noun for the rows, e.g. "bookings".</param>
    /// <returns>The pager description.</returns>
    public static PagerInfo ToPager<T>(this PagedList<T> page, string itemLabel = "records")
    {
        ArgumentNullException.ThrowIfNull(page);

        return new PagerInfo(
            page.PageNumber,
            page.TotalPages,
            page.TotalCount,
            page.FirstItemNumber,
            page.LastItemNumber,
            itemLabel);
    }
}
