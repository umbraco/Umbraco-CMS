using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

/// <summary>
///     Represents a request model for creating a media type.
/// </summary>
public class CreateMediaTypeRequestModel
    : CreateContentTypeWithParentRequestModelBase<CreateMediaTypePropertyTypeRequestModel, CreateMediaTypePropertyTypeContainerRequestModel>
{
    /// <summary>
    ///     Gets or sets the allowed media types that can be created under this media type.
    /// </summary>
    public IEnumerable<MediaTypeSort> AllowedMediaTypes { get; set; } = Enumerable.Empty<MediaTypeSort>();

    /// <summary>
    ///     Gets or sets the compositions for this media type.
    /// </summary>
    public IEnumerable<MediaTypeComposition> Compositions { get; set; } = Enumerable.Empty<MediaTypeComposition>();
}
