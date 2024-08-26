using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Responsible for seeding the in-memory navigation structures at application's startup
///     by rebuild the navigation structures.
/// </summary>
public sealed class NavigationInitializationService : IHostedLifecycleService
{
    private readonly IDocumentNavigationService _documentNavigationService;
    private readonly IMediaNavigationService _mediaNavigationService;

    public NavigationInitializationService(IDocumentNavigationService documentNavigationService, IMediaNavigationService mediaNavigationService)
    {
        _documentNavigationService = documentNavigationService;
        _mediaNavigationService = mediaNavigationService;
    }

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        await _documentNavigationService.RebuildAsync();
        await _documentNavigationService.RebuildBinAsync();
        await _mediaNavigationService.RebuildAsync();
        await _mediaNavigationService.RebuildBinAsync();
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
