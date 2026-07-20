namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a system field that a node's children can be sorted by.
/// </summary>
public enum ContentSortField
{
    /// <summary>
    ///     Sort by the node's name.
    /// </summary>
    Name,

    /// <summary>
    ///     Sort by the date the node was created.
    /// </summary>
    CreateDate,

    /// <summary>
    ///     Sort by the date the node was last updated.
    /// </summary>
    UpdateDate,
}
