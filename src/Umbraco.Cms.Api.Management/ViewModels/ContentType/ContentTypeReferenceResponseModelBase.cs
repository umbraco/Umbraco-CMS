namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Serves as the base class for response models that reference content types in the API.
/// </summary>
public abstract class ContentTypeReferenceResponseModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the content type reference.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the icon associated with the content type.
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a reference to the collection associated with this content type, identified by ID.
    /// </summary>
    public ReferenceByIdModel? Collection { get; set; }
}
