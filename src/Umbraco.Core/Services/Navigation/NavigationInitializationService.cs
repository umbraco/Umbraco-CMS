using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Core.Services.Navigation;

public class NavigationInitializationService : IHostedLifecycleService
{
    private readonly INavigationService _navigationService;

    public NavigationInitializationService(INavigationService navigationService)
        => _navigationService = navigationService;

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        await _navigationService.RebuildAsync();
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
