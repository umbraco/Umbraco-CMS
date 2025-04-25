using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;

namespace Umbraco.Cms.Api.Management.ViewModels.Media;

public class MediaResponseModel : ContentResponseModelBase<MediaValueResponseModel, MediaVariantResponseModel>
{
    [Obsolete("This property is no longer populated. Please use /media/{id}/urls instead to retrieve the URLs for a document. Scheduled for removal in Umbraco 17.")]
    public IEnumerable<MediaUrlInfo> Urls { get; set; } = Enumerable.Empty<MediaUrlInfo>();

    public bool IsTrashed { get; set; }

    public MediaTypeReferenceResponseModel MediaType { get; set; } = new();
}
