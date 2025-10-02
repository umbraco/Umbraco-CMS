using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
///    A hosted service that checks for any runnable distributed background jobs on a timer.
/// </summary>
public class DistributedBackgroundJobHostedService : BackgroundService
{
    private readonly ILogger<DistributedBackgroundJobHostedService> _logger;
    private readonly IRuntimeState _runtimeState;
    private readonly IDistributedJobRepository _distributedJobRepository;
    private readonly IEnumerable<IDistributedBackgroundJob> _distributedBackgroundJobs;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public DistributedBackgroundJobHostedService(
        ILogger<DistributedBackgroundJobHostedService> logger,
        IRuntimeState runtimeState,
        IDistributedJobRepository distributedJobRepository,
        IEnumerable<IDistributedBackgroundJob> distributedBackgroundJobs,
        ICoreScopeProvider coreScopeProvider)
    {
        _logger = logger;
        _runtimeState = runtimeState;
        _distributedJobRepository = distributedJobRepository;
        _distributedBackgroundJobs = distributedBackgroundJobs;
        _coreScopeProvider = coreScopeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWork();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");
        }
    }

    private async Task DoWork()
    {
        // Do not run distributed jobs if we aren't in Run level, as we might not have booted properly.
        if(_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }


        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();


        var jobName = _distributedJobRepository.GetRunnableJob();

        if (jobName is null)
        {
            // No runnable jobs for now, return
            return;
        }

        // Try to find the job by name
        IDistributedBackgroundJob? job = _distributedBackgroundJobs.FirstOrDefault(x => x.Name == jobName);

        if (job is null)
        {
            // Could not find the job, log..
            _logger.LogCritical("Could not find a distributed job with the name '{JobName}'", jobName);
            return;
        }

        await job.RunJobAsync();

        scope.Complete();
    }
}
