using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Navigation;

internal sealed class MediaNavigationService : ContentNavigationServiceBase, IMediaNavigationService
{
    public MediaNavigationService(ICoreScopeProvider coreScopeProvider, INavigationRepository navigationRepository)
        : base(coreScopeProvider, navigationRepository)
    {
    }

    public override async Task RebuildAsync()
        => await HandleRebuildAsync(Constants.Locks.MediaTree, Constants.ObjectTypes.Media, false);

    public override async Task RebuildBinAsync()
        => await HandleRebuildAsync(Constants.Locks.MediaTree, Constants.ObjectTypes.Media, true);
}
