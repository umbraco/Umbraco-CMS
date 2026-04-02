// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Cms.Infrastructure.HostedServices;

/// <summary>
/// Provides a base class for recurring background tasks implemented as hosted services.
/// </summary>
public abstract class RecurringHostedServiceBase : BackgroundService
{
    /// <summary>
    /// The default delay to use for recurring tasks for the first run after application start-up if no alternative is configured.
    /// </summary>
    protected static readonly TimeSpan DefaultDelay = TimeSpan.FromMinutes(3);

    private readonly TimeSpan _delay;
    private readonly ILogger? _logger;
    private readonly SemaphoreSlim _signal = new(0, 1);
    private TimeSpan _period;
    private volatile TriggerState _triggerState = TriggerState.Default;
    private volatile bool _nextExecutionSkipOnOvershoot;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecurringHostedServiceBase" /> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="period">Timespan representing how often the task should recur.</param>
    /// <param name="delay">Timespan representing the initial delay after application start-up before the first run of the task occurs.</param>
    protected RecurringHostedServiceBase(ILogger? logger, TimeSpan period, TimeSpan delay)
    {
        _logger = logger;
        _period = period;
        _delay = delay;
    }

    /// <summary>
    /// Determines the delay before the first run of a recurring task implemented as a hosted service when an optional configuration for the first run time is available.
    /// </summary>
    /// <param name="firstRunTime">The configured time to first run the task in crontab format.</param>
    /// <param name="cronTabParser">An instance of <see cref="ICronTabParser" />.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="defaultDelay">The default delay to use when a first run time is not configured.</param>
    /// <returns>
    /// The delay before first running the recurring task.
    /// </returns>
    protected static TimeSpan GetDelay(string firstRunTime, ICronTabParser cronTabParser, ILogger logger, TimeSpan defaultDelay)
        => GetDelay(firstRunTime, cronTabParser, logger, DateTime.Now, defaultDelay);

    /// <summary>
    /// Determines the delay before the first run of a recurring task implemented as a hosted service when an optional configuration for the first run time is available.
    /// </summary>
    /// <param name="firstRunTime">The configured time to first run the task in crontab format.</param>
    /// <param name="cronTabParser">An instance of <see cref="ICronTabParser" />.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="now">The current datetime.</param>
    /// <param name="defaultDelay">The default delay to use when a first run time is not configured.</param>
    /// <returns>
    /// The delay before first running the recurring task.
    /// </returns>
    /// <remarks>
    /// Internal to expose for unit tests.
    /// </remarks>
    internal static TimeSpan GetDelay(string firstRunTime, ICronTabParser cronTabParser, ILogger logger, DateTime now, TimeSpan defaultDelay)
    {
        // If first run time not set, start with just small delay after application start.
        if (string.IsNullOrEmpty(firstRunTime))
        {
            return defaultDelay;
        }

        // If first run time not a valid cron tab, log, and revert to small delay after application start.
        if (!cronTabParser.IsValidCronTab(firstRunTime))
        {
            logger.LogWarning("Could not parse {FirstRunTime} as a crontab expression. Defaulting to default delay for hosted service start.", firstRunTime);

            return defaultDelay;
        }

        // Otherwise start at scheduled time according to cron expression, unless within the default delay period.
        DateTime firstRunOccurrence = cronTabParser.GetNextOccurrence(firstRunTime, now);
        TimeSpan delay = firstRunOccurrence - now;

        return delay < defaultDelay
            ? defaultDelay
            : delay;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial delay (also interruptible via signal)
        if (_delay > TimeSpan.Zero)
        {
            try
            {
                bool signaled = await _signal.WaitAsync(_delay, stoppingToken);
                if (signaled)
                {
                    // Trigger interrupted the initial delay — consume the trigger state, so it doesn't leak into WaitForNextExecutionAsync after the first execution
                    Interlocked.Exchange(ref _triggerState, TriggerState.Default);
                    _nextExecutionSkipOnOvershoot = false;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }

        TimeSpan nextDelayBasis = _period;

        while (!stoppingToken.IsCancellationRequested)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await PerformExecuteAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                ILogger logger = _logger ?? StaticApplicationLogging.CreateLogger(GetType());
                logger.LogError(ex, "Unhandled exception in recurring hosted service.");
            }
            finally
            {
                sw.Stop();
            }

            nextDelayBasis = await WaitForNextExecutionAsync(nextDelayBasis, sw.Elapsed, stoppingToken);
        }
    }

    /// <summary>
    /// Waits for the remaining period (minus execution time) before the next execution.
    /// If <see cref="TriggerExecution()" /> is called, the wait exits immediately and returns the delay basis for the execution after the triggered one.
    /// </summary>
    /// <param name="delayBasis">The delay basis.</param>
    /// <param name="executionElapsed">The execution elapsed.</param>
    /// <param name="stoppingToken">The stopping token.</param>
    /// <returns>
    /// The delay basis to use for the next wait cycle.
    /// </returns>
    private async Task<TimeSpan> WaitForNextExecutionAsync(TimeSpan delayBasis, TimeSpan executionElapsed, CancellationToken stoppingToken)
    {
        TimeSpan delay = ComputeNextDelay(delayBasis, executionElapsed);

        // If the delay basis was from a NextExecutionStrategy.None trigger and the execution overshot the scheduled time,
        // advance to the next period tick instead of executing immediately.
        // The flag is consumed unconditionally so it never leaks into later cycles.
        bool skipOnOvershoot = _nextExecutionSkipOnOvershoot;
        _nextExecutionSkipOnOvershoot = false;

        if (delay <= TimeSpan.Zero && skipOnOvershoot)
        {
            delay = ComputeNextDelay(delayBasis + _period, executionElapsed);
        }

        if (delay <= TimeSpan.Zero)
        {
            return _period;
        }

        var waitSw = Stopwatch.StartNew();
        bool signaled;
        try
        {
            signaled = await _signal.WaitAsync(delay, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return _period;
        }

        waitSw.Stop();

        if (!signaled)
        {
            return _period; // Normal timeout — next wait uses normal period
        }

        TriggerState triggerState = Interlocked.Exchange(ref _triggerState, TriggerState.Default);
        if (triggerState.Delay.HasValue)
        {
            return triggerState.Delay.Value;
        }

        TimeSpan remaining = ComputeNextDelay(delay, waitSw.Elapsed);

        switch (triggerState.Strategy)
        {
            case NextExecutionStrategy.None:
                _nextExecutionSkipOnOvershoot = true;
                return remaining;
            case NextExecutionStrategy.Replace:
                return remaining + _period;
            case NextExecutionStrategy.Reset:
            default:
                return _period;
        }
    }

    /// <summary>
    /// Implements the work of the recurring task.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token that is signaled when the host is shutting down.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    public virtual Task PerformExecuteAsync(CancellationToken stoppingToken)
#pragma warning disable CS0618 // Type or member is obsolete
        => PerformExecuteAsync((object?)null);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Implements the work of the recurring task.
    /// </summary>
    /// <param name="state">The task state.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    [Obsolete("Override PerformExecuteAsync(CancellationToken) instead. Scheduled for removal in Umbraco 19.")]
    public virtual Task PerformExecuteAsync(object? state) => Task.CompletedTask;

    /// <summary>
    /// Executes the task.
    /// </summary>
    /// <param name="state">The task state.</param>
    [Obsolete("No longer used. The base class now uses BackgroundService.ExecuteAsync(CancellationToken). Scheduled for removal in Umbraco 19.")]
    public virtual void ExecuteAsync(object? state)
    { }

    /// <summary>
    /// Computes the delay before the next execution, subtracting the elapsed execution time from the period to prevent drift.
    /// </summary>
    /// <param name="period">The configured period between executions.</param>
    /// <param name="elapsed">The elapsed time of the current execution.</param>
    /// <returns>
    /// The remaining time before the next execution should start.
    /// </returns>
    /// <remarks>
    /// Internal to expose for unit tests.
    /// </remarks>
    internal static TimeSpan ComputeNextDelay(TimeSpan period, TimeSpan elapsed)
    {
        TimeSpan remaining = period - elapsed;

        return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }

    /// <summary>
    /// Change the period between operations. The new period takes effect on the next wait cycle.
    /// </summary>
    /// <param name="newPeriod">The new period between tasks.</param>
    protected void ChangePeriod(TimeSpan newPeriod) => _period = newPeriod;

    /// <summary>
    /// Signals the background loop to execute immediately.
    /// After the triggered execution, the original schedule is kept.
    /// If the scheduled time has already passed during the triggered execution, it is skipped and the next period tick is awaited.
    /// </summary>
    public void TriggerExecution()
        => TriggerExecution(NextExecutionStrategy.None);

    /// <summary>
    /// Signals the background loop to execute immediately, with the specified strategy for determining the next execution after the triggered one completes.
    /// </summary>
    /// <param name="strategy">Controls the delay after the triggered execution.</param>
    public void TriggerExecution(NextExecutionStrategy strategy)
    {
        _triggerState = new TriggerState(Strategy: strategy);
        ReleaseSignal();
    }

    /// <summary>
    /// Signals the background loop to execute immediately.
    /// After the triggered execution, the loop waits for the specified delay before the next execution.
    /// </summary>
    /// <param name="nextDelay">The delay to wait after the triggered execution completes (execution time is subtracted to prevent drift).</param>
    public void TriggerExecution(TimeSpan nextDelay)
    {
        _triggerState = new TriggerState(Delay: nextDelay);
        ReleaseSignal();
    }

    private void ReleaseSignal()
    {
        if (_signal.CurrentCount == 0)
        {
            try
            {
                _signal.Release();
            }
            catch (SemaphoreFullException)
            {
                // Already signaled
            }
        }
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and optionally managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _signal.Dispose();
        }

        base.Dispose();
    }

    /// <summary>
    /// Immutable snapshot of the trigger state.
    /// </summary>
    private sealed record TriggerState(NextExecutionStrategy Strategy = default, TimeSpan? Delay = null)
    {
        public static TriggerState Default { get; } = new();
    }
}
