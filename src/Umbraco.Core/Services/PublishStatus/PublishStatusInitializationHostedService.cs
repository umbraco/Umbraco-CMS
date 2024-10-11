using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Responsible for seeding the in-memory publish status cache at application's startup
///     by loading all data from the database.
/// </summary>
public sealed class PublishStatusInitializationHostedService : IHostedLifecycleService
{
    private readonly IRuntimeState _runtimeState;
    private readonly IPublishStatusManagementService _publishStatusManagementService;

    public PublishStatusInitializationHostedService(
        IRuntimeState runtimeState,
        IPublishStatusManagementService publishStatusManagementService
        )
    {
        _runtimeState = runtimeState;
        _publishStatusManagementService = publishStatusManagementService;
    }

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        if(_runtimeState.Level < RuntimeLevel.Upgrade)
        {
            return;
        }

        await _publishStatusManagementService.InitializeAsync(cancellationToken);
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
