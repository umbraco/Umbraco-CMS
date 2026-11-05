using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods to create and configure media collection presentation objects.
/// </summary>
public class MediaCollectionPresentationFactory : ContentCollectionPresentationFactory<IMedia, MediaCollectionResponseModel, MediaValueResponseModel, MediaVariantResponseModel>, IMediaCollectionPresentationFactory
{
    private readonly IMediaNavigationQueryService _mediaNavigationQueryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaCollectionPresentationFactory"/> class, which is responsible for creating media collection presentation models.
    /// </summary>
    /// <param name="mapper">An <see cref="IUmbracoMapper"/> instance used to map domain objects to presentation models.</param>
    /// <param name="flagProviders">A collection of <see cref="FlagProviderCollection"/> used to provide additional flags or metadata for media items.</param>
    /// <param name="userService">An <see cref="IUserService"/> used to perform user-related operations, such as permissions checks.</param>
    /// <param name="mediaNavigationQueryService">The service used to resolve which media items have children.</param>
    public MediaCollectionPresentationFactory(IUmbracoMapper mapper, FlagProviderCollection flagProviders, IUserService userService, IMediaNavigationQueryService mediaNavigationQueryService)
        : base(mapper, flagProviders, userService)
        => _mediaNavigationQueryService = mediaNavigationQueryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaCollectionPresentationFactory"/> class, which is responsible for creating media collection presentation models.
    /// </summary>
    /// <param name="mapper">An <see cref="IUmbracoMapper"/> instance used to map domain objects to presentation models.</param>
    /// <param name="flagProviders">A collection of <see cref="FlagProviderCollection"/> used to provide additional flags or metadata for media items.</param>
    /// <param name="userService">An <see cref="IUserService"/> used to perform user-related operations, such as permissions checks.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public MediaCollectionPresentationFactory(IUmbracoMapper mapper, FlagProviderCollection flagProviders, IUserService userService)
        : this(
            mapper,
            flagProviders,
            userService,
            StaticServiceProvider.Instance.GetRequiredService<IMediaNavigationQueryService>())
    {
    }

    /// <inheritdoc/>
    protected override INavigationQueryService? NavigationQueryService => _mediaNavigationQueryService;

    /// <inheritdoc/>
    protected override IRecycleBinNavigationQueryService? RecycleBinNavigationQueryService => _mediaNavigationQueryService;
}
