using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Media;

public class MediaCreateRequestModel : ContentCreateRequestModelBase<MediaValueModel, MediaVariantRequestModel>
{
    public Guid ContentTypeKey { get; set; }
}
