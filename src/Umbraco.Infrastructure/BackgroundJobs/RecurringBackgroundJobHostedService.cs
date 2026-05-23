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
    private readonly TimeProvider _timeProvider;
    private CancellationTokenSource _ignoredDelayChangeCts = new();

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
    /// <param name="timeProvider">The time provider used for scheduling and elapsed time measurement.</param>
    public RecurringBackgroundJobHostedService(
        IRuntimeState runtimeState,
        ILogger<RecurringBackgroundJobHostedService<TJob>> logger,
        IMainDom mainDom,
        IServerRoleAccessor serverRoleAccessor,
        IEventAggregator eventAggregator,
        IEventMessagesFactory eventMessagesFactory,
        TJob job,
        TimeProvider timeProvider)
        : base(logger, job.Period, job.Delay, timeProvider)
    {
        _runtimeState = runtimeState;
        _logger = logger;
        _mainDom = mainDom;
        _serverRoleAccessor = serverRoleAccessor;
        _eventAggregator = eventAggregator;
        _eventMessagesFactory = eventMessagesFactory;
        _job = job;
        _timeProvider = timeProvider;

        _job.PeriodChanged += OnPeriodChanged;
        _job.IgnoredDelayChanged += OnIgnoredDelayChanged;
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
    [Obsolete("Use the constructor accepting IEventMessagesFactory and TimeProvider instead. Scheduled for removal in Umbraco 19.")]
    public RecurringBackgroundJobHostedService(
        IRuntimeState runtimeState,
        ILogger<RecurringBackgroundJobHostedService<TJob>> logger,
        IMainDom mainDom,
        IServerRoleAccessor serverRoleAccessor,
        IEventAggregator eventAggregator,
        TJob job)
        : this(runtimeState, logger, mainDom, serverRoleAccessor, eventAggregator, StaticServiceProvider.Instance.GetRequiredService<IEventMessagesFactory>(), job, TimeProvider.System)
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
                await IgnoreAndWaitAsync("Job not running as runlevel not yet ready", eventMessages, executingNotification, stoppingToken);
                return;
            }

            // Don't run on replicas nor unknown role servers
            if (!_job.ServerRoles.Contains(_serverRoleAccessor.CurrentServerRole))
            {
                await IgnoreAndWaitAsync("Job not running on this server role", eventMessages, executingNotification, stoppingToken);
                return;
            }

            // Ensure we do not run if not main domain, but do NOT lock it
            if (!_mainDom.IsMainDom)
            {
                await IgnoreAndWaitAsync("Job not running as not MainDom", eventMessages, executingNotification, stoppingToken);
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

        // Suppress execution context flow around base.StartAsync so the fire-and-forget ExecuteAsync loop
        // does not capture AsyncLocal state from the host — in particular Umbraco's static AmbientScopeStack,
        // which uses a ConcurrentStack<IScope> reference that, once non-null, would be shared across every
        // hosted service that inherits this ExecutionContext. Without this, concurrent scope pushes/pops
        // across recurring loops and other hosted services interleave and trigger "not the ambient scope"
        // errors at Scope.Dispose (see DistributedJobService.EnsureJobsAsync for the original repro).
        Task startTask;
        using (ExecutionContext.IsFlowSuppressed() ? null : (IDisposable?)ExecutionContext.SuppressFlow())
        {
            startTask = base.StartAsync(cancellationToken);
        }

        await startTask;

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

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _job.PeriodChanged -= OnPeriodChanged;
            _job.IgnoredDelayChanged -= OnIgnoredDelayChanged;

            _ignoredDelayChangeCts.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Handles the <see cref="IRecurringBackgroundJob.PeriodChanged" /> event by updating the base class period.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void OnPeriodChanged(object? sender, EventArgs e)
        => ChangePeriod(_job.Period);

    /// <summary>
    /// Handles the <see cref="IRecurringBackgroundJob.IgnoredDelayChanged" /> event by interrupting any in-progress ignored back-off so it re-reads the new <see cref="IRecurringBackgroundJob.IgnoredDelay" /> value.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void OnIgnoredDelayChanged(object? sender, EventArgs e)
    {
        // Rotate without disposing — the wait loop may still be registering against the old token.
        // The old CTS is small once cancelled and will be collected by the GC.
        CancellationTokenSource oldCts = Interlocked.Exchange(ref _ignoredDelayChangeCts, new CancellationTokenSource());

        try
        {
            oldCts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Race during shutdown: Dispose disposed the CTS between this in-flight handler reading it and calling Cancel. Safe to swallow — the wait loop is already torn down.
        }
    }

    /// <summary>
    /// Publishes the ignored notification and waits for <see cref="IRecurringBackgroundJob.IgnoredDelay" /> before allowing the next iteration, preventing tight looping when execution is skipped.
    /// </summary>
    /// <param name="message">The full debug message describing why the execution is ignored.</param>
    /// <param name="eventMessages">The event messages for the notification.</param>
    /// <param name="executingNotification">The originating executing notification to carry state from.</param>
    /// <param name="stoppingToken">A cancellation token that is signaled when the host is shutting down.</param>
    private async Task IgnoreAndWaitAsync(
        string message,
        EventMessages eventMessages,
        RecurringBackgroundJobExecutingNotification executingNotification,
        CancellationToken stoppingToken)
    {
        _logger.LogDebug(message);
        await _eventAggregator.PublishAsync(new RecurringBackgroundJobIgnoredNotification(_job, eventMessages).WithStateFrom(executingNotification), stoppingToken);

        long waitStart = _timeProvider.GetTimestamp();

        while (true)
        {
            TimeSpan ignoredDelay = _job.IgnoredDelay;

            // Skip the back-off for zero and any other non-positive value (other than Timeout.InfiniteTimeSpan, which means "wait until shutdown / IgnoredDelayChanged"). Validating here defends against direct IRecurringBackgroundJob implementations or property overrides that bypass the RecurringBackgroundJobBase setter validation.
            if (ignoredDelay != Timeout.InfiniteTimeSpan && ignoredDelay <= TimeSpan.Zero)
            {
                return;
            }

            TimeSpan remaining = ComputeNextDelay(ignoredDelay, _timeProvider.GetElapsedTime(waitStart));
            if (remaining == TimeSpan.Zero)
            {
                return;
            }

            CancellationToken ignoredDelayChangeToken = _ignoredDelayChangeCts.Token;
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, ignoredDelayChangeToken);

            try
            {
                await Task.Delay(remaining, _timeProvider, linkedCts.Token);

                // Back-off complete
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Back-off interrupted by shutdown; the ignored notification has already been published, so do not also publish canceled
                return;
            }
            catch (OperationCanceledException) when (ignoredDelayChangeToken.IsCancellationRequested)
            {
                // IgnoredDelay changed — loop to re-read and recompute the remaining wait
            }
        }
    }
}
