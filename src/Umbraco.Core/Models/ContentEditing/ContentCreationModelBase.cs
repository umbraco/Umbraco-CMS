namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the base model for content creation operations.
/// </summary>
public abstract class ContentCreationModelBase : ContentEditingModelBase
{
    /// <summary>
    ///     Gets or sets the optional unique key for the content being created.
    /// </summary>
    public Guid? Key { get; set; }

    /// <summary>
    ///     Gets or sets the key of the content type for the content being created.
    /// </summary>
    public Guid ContentTypeKey { get; set; } = Guid.Empty;

    /// <summary>
    ///     Gets or sets the optional key of the parent content item.
    /// </summary>
    public Guid? ParentKey { get; set; }
}
