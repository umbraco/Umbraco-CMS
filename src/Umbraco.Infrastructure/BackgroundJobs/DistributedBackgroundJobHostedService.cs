using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Services;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
///    A hosted service that checks for any runnable distributed background jobs on a timer.
/// </summary>
public class DistributedBackgroundJobHostedService : BackgroundService
{
    private readonly ILogger<DistributedBackgroundJobHostedService> _logger;
    private readonly IRuntimeState _runtimeState;
    private readonly IDistributedJobService _distributedJobService;
    private readonly IEnumerable<IDistributedBackgroundJob> _distributedBackgroundJobs;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedBackgroundJobHostedService"/> class.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="runtimeState"></param>
    /// <param name="distributedJobService"></param>
    /// <param name="distributedBackgroundJobs"></param>
    /// <param name="coreScopeProvider"></param>
    public DistributedBackgroundJobHostedService(
        ILogger<DistributedBackgroundJobHostedService> logger,
        IRuntimeState runtimeState,
        IDistributedJobService distributedJobService,
        IEnumerable<IDistributedBackgroundJob> distributedBackgroundJobs)
    {
        _logger = logger;
        _runtimeState = runtimeState;
        _distributedJobService = distributedJobService;
        _distributedBackgroundJobs = distributedBackgroundJobs;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunRunnableJob();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");
        }
    }

    private async Task RunRunnableJob()
    {
        // Do not run distributed jobs if we aren't in Run level, as we might not have booted properly.
        if(_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        var jobName = await _distributedJobService.TryTakeRunnableJobAsync();

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

        await _distributedJobService.FinishJobAsync(job.Name);
    }
}
