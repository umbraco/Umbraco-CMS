using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods to create and configure media collection presentation objects.
/// </summary>
public class MediaCollectionPresentationFactory : ContentCollectionPresentationFactory<IMedia, MediaCollectionResponseModel, MediaValueResponseModel, MediaVariantResponseModel>, IMediaCollectionPresentationFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaCollectionPresentationFactory"/> class, which is responsible for creating media collection presentation models.
    /// </summary>
    /// <param name="mapper">An <see cref="IUmbracoMapper"/> instance used to map domain objects to presentation models.</param>
    /// <param name="flagProviders">A collection of <see cref="FlagProviderCollection"/> used to provide additional flags or metadata for media items.</param>
    /// <param name="userService">An <see cref="IUserService"/> used to perform user-related operations, such as permissions checks.</param>
    public MediaCollectionPresentationFactory(IUmbracoMapper mapper, FlagProviderCollection flagProviders, IUserService userService)
        : base(mapper, flagProviders, userService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaCollectionPresentationFactory"/> class with the specified mapper.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used for object mapping.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
    public MediaCollectionPresentationFactory(IUmbracoMapper mapper)
        : base(mapper)
    {
    }
}
