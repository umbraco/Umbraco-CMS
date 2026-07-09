namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a paged collection of items with total count information.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public class PagedModel<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PagedModel{T}" /> class.
    /// </summary>
    public PagedModel()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PagedModel{T}" /> class with the specified total and items.
    /// </summary>
    /// <param name="total">The total number of items available.</param>
    /// <param name="items">The items for the current page.</param>
    public PagedModel(long total, IEnumerable<T> items)
    {
        Total = total;
        Items = items;
    }

    /// <summary>
    ///     Gets the items for the current page.
    /// </summary>
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();

    /// <summary>
    ///     Gets the total number of items available across all pages.
    /// </summary>
    public long Total { get; init; }
}
