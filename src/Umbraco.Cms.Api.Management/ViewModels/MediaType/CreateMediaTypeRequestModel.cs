using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

[ShortGenericSchemaName<CreateMediaTypePropertyTypeRequestModel, CreateMediaTypePropertyTypeContainerRequestModel>("CreateContentTypeForMediaTypeRequestModel")]
public class CreateMediaTypeRequestModel
    : CreateContentTypeRequestModelBase<CreateMediaTypePropertyTypeRequestModel, CreateMediaTypePropertyTypeContainerRequestModel>
{
}
