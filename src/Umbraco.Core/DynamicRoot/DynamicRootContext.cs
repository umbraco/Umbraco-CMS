namespace Umbraco.Cms.Core.DynamicRoot;

/// <summary>
///     Represents the context for resolving a dynamic root, containing information about the current and parent content items.
/// </summary>
public struct DynamicRootContext
{
    /// <summary>
    ///     Gets or sets the unique identifier of the current content item, or <c>null</c> when creating new content.
    /// </summary>
    public required Guid? CurrentKey { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the parent content item.
    /// </summary>
    public required Guid ParentKey { get; set; }
}
