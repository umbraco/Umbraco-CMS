using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Responsible for seeding the in-memory navigation structures at application's startup
///     by rebuild the navigation structures.
/// </summary>
public sealed class NavigationInitializationNotificationHandler : INotificationAsyncHandler<PostRuntimePremigrationsUpgradeNotification>
{
    private readonly IRuntimeState _runtimeState;
    private readonly IDocumentNavigationManagementService _documentNavigationManagementService;
    private readonly IMediaNavigationManagementService _mediaNavigationManagementService;

    public NavigationInitializationNotificationHandler(
        IRuntimeState runtimeState,
        IDocumentNavigationManagementService documentNavigationManagementService,
        IMediaNavigationManagementService mediaNavigationManagementService)
    {
        _runtimeState = runtimeState;
        _documentNavigationManagementService = documentNavigationManagementService;
        _mediaNavigationManagementService = mediaNavigationManagementService;
    }

    public async Task HandleAsync(PostRuntimePremigrationsUpgradeNotification notification, CancellationToken cancellationToken)
    {
        if(_runtimeState.Level < RuntimeLevel.Upgrade)
        {
            return;
        }

        await _documentNavigationManagementService.RebuildAsync();
        await _documentNavigationManagementService.RebuildBinAsync();
        await _mediaNavigationManagementService.RebuildAsync();
        await _mediaNavigationManagementService.RebuildBinAsync();
    }
}
