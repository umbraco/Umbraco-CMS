using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

public class UpdateMediaTypeRequestModel
    : UpdateContentTypeRequestModelBase<UpdateMediaTypePropertyTypeRequestModel, UpdateMediaTypePropertyTypeContainerRequestModel>
{
    public IEnumerable<MediaTypeSort> AllowedMediaTypes { get; set; } = Enumerable.Empty<MediaTypeSort>();

    public IEnumerable<MediaTypeComposition> Compositions { get; set; } = Enumerable.Empty<MediaTypeComposition>();
}
