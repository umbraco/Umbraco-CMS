using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

/// <summary>
/// Represents the data required to update an existing media type via the management API.
/// </summary>
public class UpdateMediaTypeRequestModel
    : UpdateContentTypeRequestModelBase<UpdateMediaTypePropertyTypeRequestModel, UpdateMediaTypePropertyTypeContainerRequestModel>
{
    /// <summary>
    /// Gets or sets the collection of media types that are allowed as children of this media type.
    /// </summary>
    public IEnumerable<MediaTypeSort> AllowedMediaTypes { get; set; } = Enumerable.Empty<MediaTypeSort>();

    /// <summary>
    /// Gets or sets the collection of media type compositions associated with this media type.
    /// </summary>
    public IEnumerable<MediaTypeComposition> Compositions { get; set; } = Enumerable.Empty<MediaTypeComposition>();
}
