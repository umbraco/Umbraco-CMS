using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
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
    private DistributedJobSettings _distributedJobSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedBackgroundJobHostedService"/> class.
    /// </summary>
    public DistributedBackgroundJobHostedService(
        ILogger<DistributedBackgroundJobHostedService> logger,
        IRuntimeState runtimeState,
        IDistributedJobService distributedJobService,
        IOptionsMonitor<DistributedJobSettings> distributedJobSettings)
    {
        _logger = logger;
        _runtimeState = runtimeState;
        _distributedJobService = distributedJobService;
        _distributedJobSettings = distributedJobSettings.CurrentValue;
        distributedJobSettings.OnChange(options =>
        {
            _distributedJobSettings = options;
        });
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(_distributedJobSettings.Delay, stoppingToken);

        while (_runtimeState.Level != RuntimeLevel.Run)
        {
            await Task.Delay(_distributedJobSettings.Delay, stoppingToken);
        }

        try
        {
            // Update all jobs, periods might have changed when restarting.
            await _distributedJobService.EnsureJobsAsync();
        }
        catch (Exception exception)
        {
            // We swallow exception here, don't want the app to crash if something goes wrong
            _logger.LogError(exception, "An exception occurred while attempting to ensure distributed background jobs on startup.");
        }


        using PeriodicTimer timer = new(_distributedJobSettings.Period);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await RunRunnableJob();
                }
                catch (Exception exception)
                {
                    if (exception is OperationCanceledException)
                    {
                        // If the operation was canceled, just re-throw to stop the service
                        throw;
                    }

                    _logger.LogError(exception, "An exception occurred while attempting to run a distributed background job.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");
        }
    }

    private async Task RunRunnableJob()
    {
        IDistributedBackgroundJob? job = await _distributedJobService.TryTakeRunnableAsync();

        if (job is null)
        {
            // No runnable jobs for now, return
            return;
        }

        try
        {
            await job.ExecuteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while running distributed background job '{JobName}'.", job.Name);
        }
        finally
        {
            try
            {
                await _distributedJobService.FinishAsync(job.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while finishing distributed background job '{JobName}'.", job.Name);
            }
        }
    }
}
