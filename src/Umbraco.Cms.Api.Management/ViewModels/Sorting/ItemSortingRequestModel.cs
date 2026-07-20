namespace Umbraco.Cms.Api.Management.ViewModels.Sorting;

/// <summary>
/// Represents a request model used to specify the new order of items in a sorting operation.
/// </summary>
public class ItemSortingRequestModel
{
    /// <summary>
    /// Gets the unique identifier of the item to be sorted.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the position of the item in the sort order.
    /// </summary>
    public int SortOrder { get; set; }
}
