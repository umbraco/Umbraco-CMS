using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

[ShortGenericSchemaName<CreateMediaTypePropertyTypeRequestModel, CreateMediaTypePropertyTypeContainerRequestModel>("CreateContentTypeForMediaTypeRequestModel")]
public class CreateMediaTypeRequestModel
    : CreateContentTypeInFolderRequestModelBase<CreateMediaTypePropertyTypeRequestModel, CreateMediaTypePropertyTypeContainerRequestModel>
{
    public IEnumerable<MediaTypeSort> AllowedMediaTypes { get; set; } = Enumerable.Empty<MediaTypeSort>();

    public IEnumerable<MediaTypeComposition> Compositions { get; set; } = Enumerable.Empty<MediaTypeComposition>();
}
