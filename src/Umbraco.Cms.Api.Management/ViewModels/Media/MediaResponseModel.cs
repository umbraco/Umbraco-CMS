using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;

namespace Umbraco.Cms.Api.Management.ViewModels.Media;

public class MediaResponseModel : ContentResponseModelBase<MediaValueResponseModel, MediaVariantResponseModel>
{
    public IEnumerable<MediaUrlInfo> Urls { get; set; } = Enumerable.Empty<MediaUrlInfo>();

    public bool IsTrashed { get; set; }

    public MediaTypeReferenceResponseModel MediaType { get; set; } = new();
}
