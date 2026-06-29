namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Serves as the base response model for content type collection references.
/// </summary>
public abstract class ContentTypeCollectionReferenceResponseModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the content type.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the alias of the content type.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon associated with the content type.
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a reference to the content type collection by its identifier. Returns <c>null</c> if no collection is associated.
    /// </summary>
    public ReferenceByIdModel? Collection { get; set; }
}
