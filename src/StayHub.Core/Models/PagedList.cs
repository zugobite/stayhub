namespace StayHub.Core.Models;

/// <summary>
/// A single page of results together with the counts a pager needs to render.
/// </summary>
/// <typeparam name="T">The row type.</typeparam>
public class PagedList<T>
{
    /// <summary>The number of rows shown on one page across the application.</summary>
    public const int DefaultPageSize = 15;

    private PagedList(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    /// <summary>Gets the rows on this page.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>Gets the one-based page number.</summary>
    public int PageNumber { get; }

    /// <summary>Gets the maximum rows per page.</summary>
    public int PageSize { get; }

    /// <summary>Gets the total number of rows across every page.</summary>
    public int TotalCount { get; }

    /// <summary>Gets the number of pages, at least one even when empty.</summary>
    public int TotalPages => TotalCount == 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Gets a value indicating whether a previous page exists.</summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>Gets a value indicating whether a next page exists.</summary>
    public bool HasNext => PageNumber < TotalPages;

    /// <summary>Gets the one-based index of the first row shown, or 0 when empty.</summary>
    public int FirstItemNumber => TotalCount == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;

    /// <summary>Gets the one-based index of the last row shown.</summary>
    public int LastItemNumber => Math.Min(PageNumber * PageSize, TotalCount);

    /// <summary>Gets a value indicating whether the pager is worth rendering at all.</summary>
    public bool HasMultiplePages => TotalPages > 1;

    /// <summary>
    /// Slices an already-materialised sequence into a page.
    /// </summary>
    /// <param name="source">The full sequence.</param>
    /// <param name="pageNumber">The requested one-based page. Out-of-range values are clamped.</param>
    /// <param name="pageSize">The page size. Defaults to <see cref="DefaultPageSize"/>.</param>
    /// <returns>The requested page.</returns>
    /// <remarks>
    /// Use this only where the full set is small and already loaded. For tables that
    /// grow without bound, page in the database instead and build the result with
    /// <see cref="From"/> so the query never materialises every row.
    /// </remarks>
    public static PagedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize = DefaultPageSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        var all = source as IReadOnlyList<T> ?? source.ToList();
        var size = pageSize < 1 ? DefaultPageSize : pageSize;
        var page = Clamp(pageNumber, all.Count, size);

        var items = all.Skip((page - 1) * size).Take(size).ToList();
        return new PagedList<T>(items, page, size, all.Count);
    }

    /// <summary>
    /// Wraps a page that has already been fetched, for callers that paged in the
    /// database and know the total separately.
    /// </summary>
    /// <param name="items">The rows on this page.</param>
    /// <param name="pageNumber">The one-based page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total row count across all pages.</param>
    /// <returns>The page.</returns>
    public static PagedList<T> From(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);

        var size = pageSize < 1 ? DefaultPageSize : pageSize;
        return new PagedList<T>(items.ToList(), Clamp(pageNumber, totalCount, size), size, totalCount);
    }

    /// <summary>
    /// Keeps the page number inside the available range, so a bookmarked or hand-typed
    /// <c>?page=99</c> shows the last page rather than an empty table.
    /// </summary>
    public static int Clamp(int pageNumber, int totalCount, int pageSize)
    {
        var lastPage = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return Math.Clamp(pageNumber < 1 ? 1 : pageNumber, 1, lastPage);
    }
}
