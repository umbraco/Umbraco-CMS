using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Media;

public class MediaResponseModel : ContentResponseModelBase<MediaValueModel, MediaVariantResponseModel>
{
    public IEnumerable<ContentUrlInfo> Urls { get; set; } = Array.Empty<ContentUrlInfo>();

    public string Type => Constants.UdiEntityType.Media;
}
