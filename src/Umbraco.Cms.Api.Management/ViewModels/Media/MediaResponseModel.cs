using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Media;

public class MediaResponseModel : ContentResponseModelBase<MediaValueModel, MediaVariantResponseModel>
{
    public IEnumerable<string> Urls { get; set; } = Array.Empty<string>();
}
