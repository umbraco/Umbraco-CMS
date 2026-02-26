using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Provides navigation services for media items, including querying and managing
///     the media navigation structure and its recycle bin.
/// </summary>
/// <remarks>
///     This service extends <see cref="ContentNavigationServiceBase{TContentType, TContentTypeService}"/>
///     and implements both <see cref="IMediaNavigationQueryService"/> and <see cref="IMediaNavigationManagementService"/>
///     to provide a complete set of navigation operations for media content.
/// </remarks>
internal sealed class MediaNavigationService : ContentNavigationServiceBase<IMediaType, IMediaTypeService>, IMediaNavigationQueryService, IMediaNavigationManagementService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaNavigationService"/> class.
    /// </summary>
    /// <param name="coreScopeProvider">The core scope provider for database operations.</param>
    /// <param name="navigationRepository">The repository for accessing navigation data.</param>
    /// <param name="mediaTypeService">The media type service for retrieving media type information.</param>
    public MediaNavigationService(ICoreScopeProvider coreScopeProvider, INavigationRepository navigationRepository, IMediaTypeService mediaTypeService)
        : base(coreScopeProvider, navigationRepository, mediaTypeService)
    {
    }

    /// <inheritdoc />
    public override async Task RebuildAsync()
        => await HandleRebuildAsync(Constants.Locks.MediaTree, Constants.ObjectTypes.Media, false);

    /// <inheritdoc />
    public override async Task RebuildBinAsync()
        => await HandleRebuildAsync(Constants.Locks.MediaTree, Constants.ObjectTypes.Media, true);
}
