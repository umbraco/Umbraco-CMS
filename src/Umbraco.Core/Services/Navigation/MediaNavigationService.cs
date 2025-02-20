using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Navigation;

internal sealed class MediaNavigationService : ContentNavigationServiceBase<IMediaType, IMediaTypeService>, IMediaNavigationQueryService, IMediaNavigationManagementService
{
    public MediaNavigationService(ICoreScopeProvider coreScopeProvider, INavigationRepository navigationRepository, IMediaTypeService mediaTypeService)
        : base(coreScopeProvider, navigationRepository, mediaTypeService)
    {
    }

    public override async Task RebuildAsync()
        => await HandleRebuildAsync(Constants.Locks.MediaTree, Constants.ObjectTypes.Media, false);

    public override async Task RebuildBinAsync()
        => await HandleRebuildAsync(Constants.Locks.MediaTree, Constants.ObjectTypes.Media, true);
}
