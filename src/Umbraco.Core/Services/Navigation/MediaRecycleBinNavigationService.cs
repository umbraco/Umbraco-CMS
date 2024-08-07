using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Navigation;

internal sealed class MediaRecycleBinNavigationService : ContentNavigationServiceBase, IMediaRecycleBinNavigationService
{
    public MediaRecycleBinNavigationService(ICoreScopeProvider coreScopeProvider, INavigationRepository navigationRepository)
        : base(coreScopeProvider, navigationRepository)
    {
    }

    public override async Task RebuildAsync()
        => await HandleRebuildAsync(Constants.Locks.MediaTree, Constants.ObjectTypes.Media, true);
}
