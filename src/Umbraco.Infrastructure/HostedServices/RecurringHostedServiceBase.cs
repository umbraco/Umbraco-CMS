// Copyright (c) Umbraco.
// See LICENSE for more details.

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
    private readonly TimeProvider _timeProvider;
    private readonly SemaphoreSlim _signal = new(0, 1);
    private CancellationTokenSource _periodChangeCts = new();
    private TimeSpan _period;
    private volatile TriggerState _triggerState = TriggerState.Default;
    private volatile bool _nextExecutionSkipOnOvershoot;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecurringHostedServiceBase" /> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="period">Timespan representing how often the task should recur.</param>
    /// <param name="delay">Timespan representing the initial delay after application start-up before the first run of the task occurs.</param>
    /// <param name="timeProvider">The time provider used for scheduling and elapsed time measurement.</param>
    protected RecurringHostedServiceBase(ILogger? logger, TimeSpan period, TimeSpan delay, TimeProvider timeProvider)
    {
        _logger = logger;
        _period = period;
        _delay = delay;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecurringHostedServiceBase" /> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="period">Timespan representing how often the task should recur.</param>
    /// <param name="delay">Timespan representing the initial delay after application start-up before the first run of the task occurs.</param>
    [Obsolete("Use the constructor accepting TimeProvider. Scheduled for removal in Umbraco 19.")]
    protected RecurringHostedServiceBase(ILogger? logger, TimeSpan period, TimeSpan delay)
        : this(logger, period, delay, TimeProvider.System)
    { }

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
    [Obsolete("Use DelayCalculator.GetDelay instead. Scheduled for removal in Umbraco 19.")]
    protected static TimeSpan GetDelay(string firstRunTime, ICronTabParser cronTabParser, ILogger logger, TimeSpan defaultDelay)
        => BackgroundJobs.DelayCalculator.GetDelay(firstRunTime, cronTabParser, logger, defaultDelay);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial delay (also interruptible via signal)
        if (_delay > TimeSpan.Zero)
        {
            try
            {
                bool signaled = await WaitForSignalAsync(_delay, CancellationToken.None, stoppingToken);
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
            long startTimestamp = _timeProvider.GetTimestamp();
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

            TimeSpan executionElapsed = _timeProvider.GetElapsedTime(startTimestamp);
            nextDelayBasis = await WaitForNextExecutionAsync(nextDelayBasis, executionElapsed, stoppingToken);
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

        long waitStart = _timeProvider.GetTimestamp();

        while (true)
        {
            CancellationToken periodChangeToken = _periodChangeCts.Token;
            bool signaled;
            try
            {
                signaled = await WaitForSignalAsync(delay, periodChangeToken, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return _period;
            }

            if (!signaled && periodChangeToken.IsCancellationRequested)
            {
                // Period changed — recalculate remaining delay with the new period and re-wait.
                TimeSpan totalElapsed = executionElapsed + _timeProvider.GetElapsedTime(waitStart);
                delay = ComputeNextDelay(_period, totalElapsed);
                if (delay <= TimeSpan.Zero)
                {
                    return _period;
                }

                continue;
            }

            if (!signaled)
            {
                return _period; // Normal timeout — next wait uses normal period
            }

            TriggerState triggerState = Interlocked.Exchange(ref _triggerState, TriggerState.Default);
            if (triggerState.Delay.HasValue)
            {
                return triggerState.Delay.Value;
            }

            TimeSpan waitElapsed = _timeProvider.GetElapsedTime(waitStart);
            TimeSpan remaining = ComputeNextDelay(delay, waitElapsed);

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
    /// Change the period between operations. The new period takes effect immediately, interrupting the current wait if necessary.
    /// </summary>
    /// <param name="newPeriod">The new period between tasks.</param>
    protected void ChangePeriod(TimeSpan newPeriod)
    {
        _period = newPeriod;

        // Cancel but don't dispose — the wait loop may still be registering against the token.
        // The old CTS is small once cancelled and will be collected by the GC.
        CancellationTokenSource oldCts = Interlocked.Exchange(ref _periodChangeCts, new CancellationTokenSource());
        oldCts.Cancel();
    }

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

    /// <summary>
    /// Waits for the semaphore to be signaled or for the timeout to expire, using the injected <see cref="TimeProvider" />.
    /// </summary>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <param name="periodChangeToken">A cancellation token that is signaled when the period changes.</param>
    /// <param name="stoppingToken">A cancellation token for shutdown.</param>
    /// <returns>
    ///   <c>true</c> if the semaphore was signaled; <c>false</c> if the timeout expired or the period changed.
    /// </returns>
    private async Task<bool> WaitForSignalAsync(TimeSpan timeout, CancellationToken periodChangeToken, CancellationToken stoppingToken)
    {
        using var timeoutCts = new CancellationTokenSource(timeout, _timeProvider);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, periodChangeToken, stoppingToken);

        try
        {
            await _signal.WaitAsync(linkedCts.Token);
            return true;
        }
        catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
        {
            return false; // Timeout expired or period changed
        }
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
            _periodChangeCts.Dispose();
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
