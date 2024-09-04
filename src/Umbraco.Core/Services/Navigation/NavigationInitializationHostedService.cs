using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Responsible for seeding the in-memory navigation structures at application's startup
///     by rebuild the navigation structures.
/// </summary>
public sealed class NavigationInitializationHostedService : IHostedLifecycleService
{
    private readonly IRuntimeState _runtimeState;
    private readonly IDocumentNavigationManagementService _documentNavigationManagementService;
    private readonly IMediaNavigationManagementService _mediaNavigationManagementService;

    public NavigationInitializationHostedService(
        IRuntimeState runtimeState,
        IDocumentNavigationManagementService documentNavigationManagementService,
        IMediaNavigationManagementService mediaNavigationManagementService)
    {
        _runtimeState = runtimeState;
        _documentNavigationManagementService = documentNavigationManagementService;
        _mediaNavigationManagementService = mediaNavigationManagementService;
    }

    public async Task StartingAsync(CancellationToken cancellationToken)
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

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
