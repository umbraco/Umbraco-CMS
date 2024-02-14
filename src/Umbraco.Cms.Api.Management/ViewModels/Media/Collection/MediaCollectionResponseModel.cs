using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;

namespace Umbraco.Cms.Api.Management.ViewModels.Media.Collection;

public class MediaCollectionResponseModel : ContentCollectionResponseModelBase<MediaValueModel, MediaVariantResponseModel>
{
    public MediaTypeCollectionReferenceResponseModel MediaType { get; set; } = new();
}
