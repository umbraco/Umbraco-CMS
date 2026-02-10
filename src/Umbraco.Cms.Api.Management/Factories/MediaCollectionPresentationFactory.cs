using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class MediaCollectionPresentationFactory : ContentCollectionPresentationFactory<IMedia, MediaCollectionResponseModel, MediaValueResponseModel, MediaVariantResponseModel>, IMediaCollectionPresentationFactory
{
    public MediaCollectionPresentationFactory(IUmbracoMapper mapper, FlagProviderCollection flagProviders, IUserService userService)
        : base(mapper, flagProviders, userService)
    {
    }

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
    public MediaCollectionPresentationFactory(IUmbracoMapper mapper)
        : base(mapper)
    {
    }
}
