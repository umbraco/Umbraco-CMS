using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

[ShortGenericSchemaName<UpdateMediaTypePropertyTypeRequestModel, UpdateMediaTypePropertyTypeContainerRequestModel>("UpdateContentTypeForMediaTypeRequestModel")]
public class UpdateMediaTypeRequestModel
    : UpdateContentTypeRequestModelBase<UpdateMediaTypePropertyTypeRequestModel, UpdateMediaTypePropertyTypeContainerRequestModel>
{
}
