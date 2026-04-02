using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Notifications;

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
    /// <returns>
    /// A function that takes an <see cref="IRecurringBackgroundJob" /> and returns an <see cref="IHostedService" />.
    /// </returns>
    public static Func<IRecurringBackgroundJob, IHostedService> CreateHostedServiceFactory(IServiceProvider serviceProvider)
        => (IRecurringBackgroundJob job) =>
        {
            Type hostedServiceType = typeof(RecurringBackgroundJobHostedService<>).MakeGenericType(job.GetType());

            return (IHostedService)ActivatorUtilities.CreateInstance(serviceProvider, hostedServiceType, job);
        };
}

/// <summary>
/// Runs a recurring background job inside a hosted service.
/// </summary>
/// <typeparam name="TJob">The type of the job.</typeparam>
public class RecurringBackgroundJobHostedService<TJob> : RecurringHostedServiceBase
    where TJob : IRecurringBackgroundJob
{
    private readonly IRuntimeState _runtimeState;
    private readonly ILogger<RecurringBackgroundJobHostedService<TJob>> _logger;
    private readonly IMainDom _mainDom;
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private readonly IEventAggregator _eventAggregator;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly IRecurringBackgroundJob _job;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecurringBackgroundJobHostedService{TJob}" /> class, which manages the execution of a recurring background job.
    /// </summary>
    /// <param name="runtimeState">Provides information about the current runtime state of the Umbraco application.</param>
    /// <param name="logger">The logger used to record diagnostic and operational information for this hosted service.</param>
    /// <param name="mainDom">The main domain instance responsible for coordinating single-instance operations across multiple application domains.</param>
    /// <param name="serverRoleAccessor">Determines the current server's role in a multi-server environment.</param>
    /// <param name="eventAggregator">Handles the publishing and subscribing of application events.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="job">The recurring background job instance to be managed and executed by this service.</param>
    public RecurringBackgroundJobHostedService(
        IRuntimeState runtimeState,
        ILogger<RecurringBackgroundJobHostedService<TJob>> logger,
        IMainDom mainDom,
        IServerRoleAccessor serverRoleAccessor,
        IEventAggregator eventAggregator,
        IEventMessagesFactory eventMessagesFactory,
        TJob job)
        : base(logger, job.Period, job.Delay)
    {
        _runtimeState = runtimeState;
        _logger = logger;
        _mainDom = mainDom;
        _serverRoleAccessor = serverRoleAccessor;
        _eventAggregator = eventAggregator;
        _eventMessagesFactory = eventMessagesFactory;
        _job = job;

        _job.PeriodChanged += (sender, e) => ChangePeriod(_job.Period);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecurringBackgroundJobHostedService{TJob}" /> class, which manages the execution of a recurring background job.
    /// </summary>
    /// <param name="runtimeState">Provides information about the current runtime state of the Umbraco application.</param>
    /// <param name="logger">The logger used to record diagnostic and operational information for this hosted service.</param>
    /// <param name="mainDom">The main domain instance responsible for coordinating single-instance operations across multiple application domains.</param>
    /// <param name="serverRoleAccessor">Determines the current server's role in a multi-server environment.</param>
    /// <param name="eventAggregator">Handles the publishing and subscribing of application events.</param>
    /// <param name="job">The recurring background job instance to be managed and executed by this service.</param>
    [Obsolete("Use the overload accepting IEventMessagesFactory instead. This overload will be removed in Umbraco 19.")]
    public RecurringBackgroundJobHostedService(
       IRuntimeState runtimeState,
       ILogger<RecurringBackgroundJobHostedService<TJob>> logger,
       IMainDom mainDom,
       IServerRoleAccessor serverRoleAccessor,
       IEventAggregator eventAggregator,
       TJob job)
       : this(runtimeState, logger, mainDom, serverRoleAccessor, eventAggregator, StaticServiceProvider.Instance.GetRequiredService<IEventMessagesFactory>(), job)
    { }

    /// <inheritdoc />
    public override async Task PerformExecuteAsync(CancellationToken stoppingToken)
    {
        EventMessages eventMessages = _eventMessagesFactory.Get();
        var executingNotification = new RecurringBackgroundJobExecutingNotification(_job, eventMessages);
        await _eventAggregator.PublishAsync(executingNotification, stoppingToken);

        try
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
            {
                _logger.LogDebug("Job not running as runlevel not yet ready");
                await _eventAggregator.PublishAsync(new RecurringBackgroundJobIgnoredNotification(_job, eventMessages).WithStateFrom(executingNotification), stoppingToken);
                return;
            }

            // Don't run on replicas nor unknown role servers
            if (!_job.ServerRoles.Contains(_serverRoleAccessor.CurrentServerRole))
            {
                _logger.LogDebug("Job not running on this server role");
                await _eventAggregator.PublishAsync(new RecurringBackgroundJobIgnoredNotification(_job, eventMessages).WithStateFrom(executingNotification), stoppingToken);
                return;
            }

            // Ensure we do not run if not main domain, but do NOT lock it
            if (!_mainDom.IsMainDom)
            {
                _logger.LogDebug("Job not running as not MainDom");
                await _eventAggregator.PublishAsync(new RecurringBackgroundJobIgnoredNotification(_job, eventMessages).WithStateFrom(executingNotification), stoppingToken);
                return;
            }

            await _job.RunJobAsync(stoppingToken);
            await _eventAggregator.PublishAsync(new RecurringBackgroundJobExecutedNotification(_job, eventMessages).WithStateFrom(executingNotification), stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebug("Job canceled during shutdown.");
            await _eventAggregator.PublishAsync(new RecurringBackgroundJobCanceledNotification(_job, eventMessages).WithStateFrom(executingNotification), CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in recurring background job.");
            await _eventAggregator.PublishAsync(new RecurringBackgroundJobFailedNotification(_job, eventMessages).WithStateFrom(executingNotification), stoppingToken);
        }
    }

    /// <inheritdoc />
    [Obsolete("Override PerformExecuteAsync(CancellationToken) instead. Scheduled for removal in Umbraco 19.")]
    public override Task PerformExecuteAsync(object? state) => PerformExecuteAsync(CancellationToken.None);

    /// <inheritdoc />
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        EventMessages eventMessages = _eventMessagesFactory.Get();
        var startingNotification = new RecurringBackgroundJobStartingNotification(_job, eventMessages);
        await _eventAggregator.PublishAsync(startingNotification, cancellationToken);

        await base.StartAsync(cancellationToken);

        await _eventAggregator.PublishAsync(new RecurringBackgroundJobStartedNotification(_job, eventMessages).WithStateFrom(startingNotification), cancellationToken);
    }

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        EventMessages eventMessages = _eventMessagesFactory.Get();
        var stoppingNotification = new RecurringBackgroundJobStoppingNotification(_job, eventMessages);
        await _eventAggregator.PublishAsync(stoppingNotification, cancellationToken);

        await base.StopAsync(cancellationToken);

        await _eventAggregator.PublishAsync(new RecurringBackgroundJobStoppedNotification(_job, eventMessages).WithStateFrom(stoppingNotification), cancellationToken);
    }
}
