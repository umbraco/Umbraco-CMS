using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

public class DistributedBackgroundJobHostedService : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
