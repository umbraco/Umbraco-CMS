using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

[ShortGenericSchemaName<MediaTypePropertyTypeResponseModel, MediaTypePropertyTypeContainerResponseModel>("ContentTypeForMediaTypeResponseModel")]
public class MediaTypeResponseModel : ContentTypeResponseModelBase<MediaTypePropertyTypeResponseModel, MediaTypePropertyTypeContainerResponseModel>
{
    public IEnumerable<MediaTypeSort> AllowedMediaTypes { get; set; } = Enumerable.Empty<MediaTypeSort>();

    public IEnumerable<MediaTypeComposition> Compositions { get; set; } = Enumerable.Empty<MediaTypeComposition>();
}
