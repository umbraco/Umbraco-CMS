using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

/// <summary>
/// Represents a response model for a media type in the Umbraco CMS Management API.
/// </summary>
public class MediaTypeResponseModel : ContentTypeResponseModelBase<MediaTypePropertyTypeResponseModel, MediaTypePropertyTypeContainerResponseModel>
{
    /// <summary>
    /// Gets or sets the collection of media types that are allowed as children of this media type.
    /// </summary>
    public IEnumerable<MediaTypeSort> AllowedMediaTypes { get; set; } = Enumerable.Empty<MediaTypeSort>();

    /// <summary>
    /// Gets or sets the collection of media type compositions that are associated with this media type.
    /// </summary>
    public IEnumerable<MediaTypeComposition> Compositions { get; set; } = Enumerable.Empty<MediaTypeComposition>();

    /// <summary>
    /// Gets or sets a value indicating whether this media type can be deleted.
    /// </summary>
    public bool IsDeletable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the alias can be changed.
    /// </summary>
    public bool AliasCanBeChanged { get; set; }
}
