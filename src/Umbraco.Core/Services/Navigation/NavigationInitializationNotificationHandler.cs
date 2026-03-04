using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Responsible for seeding the in-memory navigation structures at application's startup
///     by rebuild the navigation structures.
/// </summary>
/// <remarks>
///     This handler listens for the <see cref="PostRuntimePremigrationsUpgradeNotification"/> and
///     triggers a rebuild of both document and media navigation structures, including their
///     respective recycle bins.
/// </remarks>
public sealed class NavigationInitializationNotificationHandler : INotificationAsyncHandler<PostRuntimePremigrationsUpgradeNotification>
{
    private readonly IRuntimeState _runtimeState;
    private readonly IDocumentNavigationManagementService _documentNavigationManagementService;
    private readonly IMediaNavigationManagementService _mediaNavigationManagementService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NavigationInitializationNotificationHandler"/> class.
    /// </summary>
    /// <param name="runtimeState">The runtime state service for checking the current runtime level.</param>
    /// <param name="documentNavigationManagementService">The document navigation management service.</param>
    /// <param name="mediaNavigationManagementService">The media navigation management service.</param>
    public NavigationInitializationNotificationHandler(
        IRuntimeState runtimeState,
        IDocumentNavigationManagementService documentNavigationManagementService,
        IMediaNavigationManagementService mediaNavigationManagementService)
    {
        _runtimeState = runtimeState;
        _documentNavigationManagementService = documentNavigationManagementService;
        _mediaNavigationManagementService = mediaNavigationManagementService;
    }

    /// <summary>
    ///     Handles the <see cref="PostRuntimePremigrationsUpgradeNotification"/> by rebuilding
    ///     the navigation structures for documents and media.
    /// </summary>
    /// <param name="notification">The notification instance.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    ///     This method only executes when the runtime level is at or above <see cref="RuntimeLevel.Upgrade"/>.
    ///     It rebuilds both the main navigation structures and the recycle bin structures for
    ///     documents and media.
    /// </remarks>
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
