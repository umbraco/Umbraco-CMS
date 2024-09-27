using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Core.Services;

public class DocumentUrlServiceInitializer : IHostedLifecycleService
{
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IRuntimeState _runtimeState;

    public DocumentUrlServiceInitializer(IDocumentUrlService documentUrlService, IRuntimeState runtimeState)
    {
        _documentUrlService = documentUrlService;
        _runtimeState = runtimeState;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_runtimeState.Level == RuntimeLevel.Upgrade)
        {
            //Special case on the first upgrade, as the database is not ready yet.
            return;
        }

        await _documentUrlService.InitAsync(
            _runtimeState.Level <= RuntimeLevel.Install,
            cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
