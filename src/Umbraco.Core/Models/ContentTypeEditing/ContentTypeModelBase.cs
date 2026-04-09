namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

/// <summary>
///     Base model for document content types, providing common properties for create and update operations.
/// </summary>
public abstract class ContentTypeModelBase : ContentTypeEditingModelBase<ContentTypePropertyTypeModel, ContentTypePropertyContainerModel>
{
    /// <summary>
    ///     Gets or sets the cleanup settings for content version history.
    /// </summary>
    public ContentTypeCleanup Cleanup { get; set; } = new();

    /// <summary>
    ///     Gets or sets the keys of the templates that are allowed for content of this type.
    /// </summary>
    public IEnumerable<Guid> AllowedTemplateKeys { get; set; } = Array.Empty<Guid>();

    /// <summary>
    ///     Gets or sets the key of the default template for content of this type.
    /// </summary>
    public Guid? DefaultTemplateKey { get; set; }
}
