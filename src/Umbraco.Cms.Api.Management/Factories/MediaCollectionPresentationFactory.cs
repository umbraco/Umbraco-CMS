using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public class MediaCollectionPresentationFactory : ContentCollectionPresentationFactory<IMedia, MediaCollectionResponseModel, MediaValueResponseModel, MediaVariantResponseModel>, IMediaCollectionPresentationFactory
{
    public MediaCollectionPresentationFactory(IUmbracoMapper mapper)
        : base(mapper)
    {
    }
}
