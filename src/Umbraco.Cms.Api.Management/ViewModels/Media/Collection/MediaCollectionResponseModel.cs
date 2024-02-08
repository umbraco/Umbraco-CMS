using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;

namespace Umbraco.Cms.Api.Management.ViewModels.Media.Collection;

public class MediaCollectionResponseModel : ContentResponseModelBase<MediaValueModel, MediaVariantResponseModel>
{
    public int SortOrder { get; set; }

    public MediaTypeCollectionReferenceResponseModel MediaType { get; set; } = new();
}
