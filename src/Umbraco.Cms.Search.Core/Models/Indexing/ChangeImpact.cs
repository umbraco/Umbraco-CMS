namespace Umbraco.Cms.Search.Core.Models.Indexing;

/// <summary>
/// Describes the scope of re-indexing a content change requires.
/// </summary>
public enum ChangeImpact
{
    /// <summary>
    /// Only the changed content item needs to be re-indexed.
    /// </summary>
    Refresh = 1,

    /// <summary>
    /// The changed content item and all of its descendants need to be re-indexed.
    /// </summary>
    RefreshWithDescendants = 2,

    /// <summary>
    /// The content item needs to be removed from the index.
    /// </summary>
    Remove = 3
}
