using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Models.Persistence;

/// <summary>
/// Represents a persisted snapshot of a previously-indexed document, used for change detection so that only actual field changes trigger re-indexing.
/// </summary>
public class IndexDocument
{
    /// <summary>
    /// Gets the key of the content item this document represents.
    /// </summary>
    public required Guid Key { get; init; }

    /// <summary>
    /// Gets the fields that were indexed for this document.
    /// </summary>
    public required IndexField[] Fields { get; init; }

    /// <summary>
    /// Gets a value indicating whether this snapshot is for the published (as opposed to draft) version.
    /// </summary>
    public required bool Published { get; init; }
}
