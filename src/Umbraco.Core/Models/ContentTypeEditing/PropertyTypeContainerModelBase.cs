namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

/// <summary>
///     Base model for property type containers such as tabs and groups.
/// </summary>
public abstract class PropertyTypeContainerModelBase
{
    /// <summary>
    ///     Gets or sets the unique key of the container.
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the key of the parent container, if any.
    /// </summary>
    public Guid? ParentKey { get; set; }

    /// <summary>
    ///     Gets or sets the name of the container.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the type of container (e.g., "Tab" or "Group").
    /// </summary>
    /// <remarks>
    ///     This needs to be a string because it can be anything in the future (not necessarily limited to "tab" or "group").
    /// </remarks>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the sort order of the container.
    /// </summary>
    public int SortOrder { get; set; }
}
