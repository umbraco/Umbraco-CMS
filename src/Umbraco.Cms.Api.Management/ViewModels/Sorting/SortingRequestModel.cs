namespace Umbraco.Cms.Api.Management.ViewModels.Sorting;

/// <summary>
/// Represents a request model used for sorting entities in the management API.
/// </summary>
public class SortingRequestModel
{
    /// <summary>
    /// Gets or sets the reference to the parent entity by its ID.
    /// </summary>
    public ReferenceByIdModel? Parent { get; init; }

    /// <summary>
    /// Gets or sets the collection of sorting requests for individual items.
    /// </summary>
    public required IEnumerable<ItemSortingRequestModel> Sorting { get; init; }
}
