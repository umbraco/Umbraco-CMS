using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Common.ViewModels.Pagination;

/// <summary>
///     Represents a subset of items with counts of items before and after the subset.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public class SubsetViewModel<T>
{
    /// <summary>
    ///     Gets or sets the total number of items before this subset.
    /// </summary>
    [Required]
    public long TotalBefore { get; set; }

    /// <summary>
    ///     Gets or sets the total number of items after this subset.
    /// </summary>
    [Required]
    public long TotalAfter { get; set; }

    /// <summary>
    ///     Gets or sets the items in the subset.
    /// </summary>
    [Required]
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    ///     Creates an empty subset view model.
    /// </summary>
    /// <returns>An empty <see cref="SubsetViewModel{T}"/> instance.</returns>
    public static SubsetViewModel<T> Empty() => new();
}
