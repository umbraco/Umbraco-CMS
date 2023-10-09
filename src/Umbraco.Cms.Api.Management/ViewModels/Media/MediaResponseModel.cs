using Umbraco.Cms.Api.Management.ViewModels.Abstract;
using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Media;

public class MediaResponseModel : ContentResponseModelBase<MediaValueModel, MediaVariantResponseModel>,
    ITracksTrashing
{
    public IEnumerable<ContentUrlInfo> Urls { get; set; } = Array.Empty<ContentUrlInfo>();
    public bool IsTrashed { get; set; }
}
