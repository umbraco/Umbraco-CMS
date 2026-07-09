using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Common.ViewModels.Pagination;

/// <summary>
///     Represents a paged collection of items with total count.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public class PagedViewModel<T>
{
    /// <summary>
    ///     Gets or sets the total number of items available.
    /// </summary>
    [Required]
    public long Total { get; set; }

    /// <summary>
    ///     Gets or sets the items in the current page.
    /// </summary>
    [Required]
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    ///     Creates an empty paged view model.
    /// </summary>
    /// <returns>An empty <see cref="PagedViewModel{T}"/> instance.</returns>
    public static PagedViewModel<T> Empty() => new();
}
