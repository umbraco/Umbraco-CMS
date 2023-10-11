using Umbraco.Cms.Api.Management.ViewModels.ContentType;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

public class MediaTypeResponseModel : ContentTypeResponseModelBase<MediaTypePropertyTypeResponseModel, MediaTypePropertyTypeContainerResponseModel>
{
    public string Type => Constants.UdiEntityType.MediaType;
}
