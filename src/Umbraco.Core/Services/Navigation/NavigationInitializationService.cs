using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Responsible for seeding the in-memory navigation structures at application's startup
///     by rebuild the navigation structures.
/// </summary>
public class NavigationInitializationService : IHostedLifecycleService
{
    private readonly IDocumentNavigationService _documentNavigationService;
    private readonly IDocumentRecycleBinNavigationService _documentRecycleBinNavigationService;
    private readonly IMediaNavigationService _mediaNavigationService;
    private readonly IMediaRecycleBinNavigationService _mediaRecycleBinNavigationService;

    public NavigationInitializationService(
        IDocumentNavigationService documentNavigationService,
        IDocumentRecycleBinNavigationService documentRecycleBinNavigationService,
        IMediaNavigationService mediaNavigationService,
        IMediaRecycleBinNavigationService mediaRecycleBinNavigationService)
    {
        _documentNavigationService = documentNavigationService;
        _documentRecycleBinNavigationService = documentRecycleBinNavigationService;
        _mediaNavigationService = mediaNavigationService;
        _mediaRecycleBinNavigationService = mediaRecycleBinNavigationService;
    }

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        await _documentNavigationService.RebuildAsync();
        await _documentNavigationService.RebuildBinAsync();

        await _documentRecycleBinNavigationService.RebuildAsync();
        await _mediaNavigationService.RebuildAsync();
        await _mediaRecycleBinNavigationService.RebuildAsync();
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
