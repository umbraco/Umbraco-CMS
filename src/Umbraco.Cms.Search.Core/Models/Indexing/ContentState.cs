namespace Umbraco.Cms.Search.Core.Models.Indexing;

/// <summary>
/// Identifies which index a content change applies to.
/// </summary>
public enum ContentState
{
    /// <summary>
    /// The draft content index.
    /// </summary>
    Draft,

    /// <summary>
    /// The published content index.
    /// </summary>
    Published
}
