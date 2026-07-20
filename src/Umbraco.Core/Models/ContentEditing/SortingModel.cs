namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a model for sorting content items.
/// </summary>
public class SortingModel
{
    /// <summary>
    ///     Gets the unique key of the content item to sort.
    /// </summary>
    public required Guid Key { get; init; }

    /// <summary>
    ///     Gets the sort order position for the content item.
    /// </summary>
    public required int SortOrder { get; init; }
}
