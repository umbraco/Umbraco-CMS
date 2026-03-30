using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Core;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
/// Hosted service responsible for scheduling and executing recurring background jobs within the application.
/// </summary>
public static class RecurringBackgroundJobHostedService
{
    /// <summary>
    /// Creates a factory function that produces hosted services for recurring background jobs.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to create hosted service instances.</param>
    /// <returns>A function that takes an <see cref="IRecurringBackgroundJob"/> and returns an <see cref="IHostedService"/>.</returns>
    public static Func<IRecurringBackgroundJob, IHostedService> CreateHostedServiceFactory(IServiceProvider serviceProvider) =>
        (IRecurringBackgroundJob job) =>
        {
            Type hostedServiceType = typeof(RecurringBackgroundJobHostedService<>).MakeGenericType(job.GetType());
            return (IHostedService)ActivatorUtilities.CreateInstance(serviceProvider, hostedServiceType, job);
        };
}

/// <summary>
/// Runs a recurring background job inside a hosted service.
/// Generic version for DependencyInjection
/// </summary>
/// <typeparam name="TJob">Type of the Job</typeparam>
public class RecurringBackgroundJobHostedService<TJob> : RecurringHostedServiceBase where TJob : IRecurringBackgroundJob
{

    private readonly ILogger<RecurringBackgroundJobHostedService<TJob>> _logger;
    private readonly IMainDom _mainDom;
    private readonly IRuntimeState _runtimeState;
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private readonly IEventAggregator _eventAggregator;
    private readonly IRecurringBackgroundJob _job;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecurringBackgroundJobHostedService{TJob}"/> class, which manages the execution of a recurring background job.
    /// </summary>
    /// <param name="runtimeState">Provides information about the current runtime state of the Umbraco application.</param>
    /// <param name="logger">The logger used to record diagnostic and operational information for this hosted service.</param>
    /// <param name="mainDom">The main domain instance responsible for coordinating single-instance operations across multiple application domains.</param>
    /// <param name="serverRoleAccessor">Determines the current server's role in a multi-server environment.</param>
    /// <param name="eventAggregator">Handles the publishing and subscribing of application events.</param>
    /// <param name="job">The recurring background job instance to be managed and executed by this service.</param>
    public RecurringBackgroundJobHostedService(
        IRuntimeState runtimeState,
        ILogger<RecurringBackgroundJobHostedService<TJob>> logger,
        IMainDom mainDom,
        IServerRoleAccessor serverRoleAccessor,
        IEventAggregator eventAggregator,
        TJob job)
        : base(logger, job.Period, job.Delay)
    {
        _runtimeState = runtimeState;
        _logger = logger;
        _mainDom = mainDom;
        _serverRoleAccessor = serverRoleAccessor;
        _eventAggregator = eventAggregator;
        _job = job;

        _job.PeriodChanged += (sender, e) => ChangePeriod(_job.Period);
    }

    /// <inheritdoc />
    public override async Task PerformExecuteAsync(object? state)
    {
        var executingNotification = new Notifications.RecurringBackgroundJobExecutingNotification(_job, new EventMessages());
        await _eventAggregator.PublishAsync(executingNotification);

        try
        {

            if (_runtimeState.Level != RuntimeLevel.Run)
            {
                _logger.LogDebug("Job not running as runlevel not yet ready");
                await _eventAggregator.PublishAsync(new Notifications.RecurringBackgroundJobIgnoredNotification(_job, new EventMessages()).WithStateFrom(executingNotification));
                return;
            }

            // Don't run on replicas nor unknown role servers
            if (!_job.ServerRoles.Contains(_serverRoleAccessor.CurrentServerRole))
            {
                _logger.LogDebug("Job not running on this server role");
                await _eventAggregator.PublishAsync(new Notifications.RecurringBackgroundJobIgnoredNotification(_job, new EventMessages()).WithStateFrom(executingNotification));
                return;
            }

            // Ensure we do not run if not main domain, but do NOT lock it
            if (!_mainDom.IsMainDom)
            {
                _logger.LogDebug("Job not running as not MainDom");
                await _eventAggregator.PublishAsync(new Notifications.RecurringBackgroundJobIgnoredNotification(_job, new EventMessages()).WithStateFrom(executingNotification));
                return;
            }


            await _job.RunJobAsync();
            await _eventAggregator.PublishAsync(new Notifications.RecurringBackgroundJobExecutedNotification(_job, new EventMessages()).WithStateFrom(executingNotification));


        }
        catch (Exception ex)
        {
            await _eventAggregator.PublishAsync(new Notifications.RecurringBackgroundJobFailedNotification(_job, new EventMessages()).WithStateFrom(executingNotification));
            _logger.LogError(ex, "Unhandled exception in recurring background job.");
        }

    }

    /// <summary>
    /// Asynchronously starts the recurring background job and publishes notifications before and after the job is started.
    /// This method first publishes a <see cref="Notifications.RecurringBackgroundJobStartingNotification"/> prior to starting the job,
    /// then calls the base implementation to start the job, and finally publishes a <see cref="Notifications.RecurringBackgroundJobStartedNotification"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var startingNotification = new Notifications.RecurringBackgroundJobStartingNotification(_job, new EventMessages());
        await _eventAggregator.PublishAsync(startingNotification);

        await base.StartAsync(cancellationToken);

        await _eventAggregator.PublishAsync(new Notifications.RecurringBackgroundJobStartedNotification(_job, new EventMessages()).WithStateFrom(startingNotification));

    }

    /// <summary>
    /// Asynchronously stops the recurring background job service, publishing notifications before and after stopping.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var stoppingNotification = new Notifications.RecurringBackgroundJobStoppingNotification(_job, new EventMessages());
        await _eventAggregator.PublishAsync(stoppingNotification);

        await base.StopAsync(cancellationToken);

        await _eventAggregator.PublishAsync(new Notifications.RecurringBackgroundJobStoppedNotification(_job, new EventMessages()).WithStateFrom(stoppingNotification));
    }
}
