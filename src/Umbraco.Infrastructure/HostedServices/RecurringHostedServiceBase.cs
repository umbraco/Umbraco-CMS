// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Cms.Infrastructure.HostedServices;

/// <summary>
///     Provides a base class for recurring background tasks implemented as hosted services.
/// </summary>
/// <remarks>
///     See: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-3.1&tabs=visual-studio#timed-background-tasks
/// </remarks>
public abstract class RecurringHostedServiceBase : IHostedService, IDisposable
{
    /// <summary>
    ///     The default delay to use for recurring tasks for the first run after application start-up if no alternative is
    ///     configured.
    /// </summary>
    protected static readonly TimeSpan DefaultDelay = TimeSpan.FromMinutes(3);

    private readonly TimeSpan _delay;

    private readonly ILogger? _logger;
    private bool _disposedValue;
    private TimeSpan _period;
    private Timer? _timer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RecurringHostedServiceBase" /> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="period">Timespan representing how often the task should recur.</param>
    /// <param name="delay">
    ///     Timespan representing the initial delay after application start-up before the first run of the task
    ///     occurs.
    /// </param>
    protected RecurringHostedServiceBase(ILogger? logger, TimeSpan period, TimeSpan delay)
    {
        _logger = logger;
        _period = period;
        _delay = delay;
    }

    // Scheduled for removal in V11
    [Obsolete("Please use constructor that takes an ILogger instead")]
    protected RecurringHostedServiceBase(TimeSpan period, TimeSpan delay)
        : this(null, period, delay)
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Determines the delay before the first run of a recurring task implemented as a hosted service when an optonal
    /// configuration for the first run time is available.
    /// </summary>
    /// <param name="firstRunTime">The configured time to first run the task in crontab format.</param>
    /// <param name="cronTabParser">An instance of <see cref="ICronTabParser"/></param>
    /// <param name="logger">The logger.</param>
    /// <param name="defaultDelay">The default delay to use when a first run time is not configured.</param>
    /// <returns>The delay before first running the recurring task.</returns>
    protected static TimeSpan GetDelay(
        string firstRunTime,
        ICronTabParser cronTabParser,
        ILogger logger,
        TimeSpan defaultDelay) => GetDelay(firstRunTime, cronTabParser, logger, DateTime.Now, defaultDelay);

    /// <summary>
    /// Determines the delay before the first run of a recurring task implemented as a hosted service when an optonal
    /// configuration for the first run time is available.
    /// </summary>
    /// <param name="firstRunTime">The configured time to first run the task in crontab format.</param>
    /// <param name="cronTabParser">An instance of <see cref="ICronTabParser"/></param>
    /// <param name="logger">The logger.</param>
    /// <param name="now">The current datetime.</param>
    /// <param name="defaultDelay">The default delay to use when a first run time is not configured.</param>
    /// <returns>The delay before first running the recurring task.</returns>
    /// <remarks>Internal to expose for unit tests.</remarks>
    internal static TimeSpan GetDelay(
        string firstRunTime,
        ICronTabParser cronTabParser,
        ILogger logger,
        DateTime now,
        TimeSpan defaultDelay)
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
        DateTime firstRunOccurance = cronTabParser.GetNextOccurrence(firstRunTime, now);
        TimeSpan delay = firstRunOccurance - now;
        return delay < defaultDelay
            ? defaultDelay
            : delay;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using (!ExecutionContext.IsFlowSuppressed() ? (IDisposable)ExecutionContext.SuppressFlow() : null)
        {
            _timer = new Timer(ExecuteAsync, null, (int)_delay.TotalMilliseconds, (int)_period.TotalMilliseconds);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _period = Timeout.InfiniteTimeSpan;
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Executes the task.
    /// </summary>
    /// <param name="state">The task state.</param>
    public async void ExecuteAsync(object? state)
    {
        try
        {
            // First, stop the timer, we do not want tasks to execute in parallel
            _timer?.Change(Timeout.Infinite, 0);

            // Delegate work to method returning a task, that can be called and asserted in a unit test.
            // Without this there can be behaviour where tests pass, but an error within them causes the test
            // running process to crash.
            // Hat-tip: https://stackoverflow.com/a/14207615/489433
            await PerformExecuteAsync(state);
        }
        catch (Exception ex)
        {
            ILogger logger = _logger ?? StaticApplicationLogging.CreateLogger(GetType());
            logger.LogError(ex, "Unhandled exception in recurring hosted service.");
        }
        finally
        {
            // Resume now that the task is complete - Note we use period in both because we don't want to execute again after the delay.
            // So first execution is after _delay, and the we wait _period between each
            _timer?.Change((int)_period.TotalMilliseconds, (int)_period.TotalMilliseconds);
        }
    }

    public abstract Task PerformExecuteAsync(object? state);

    /// <summary>
    ///     Change the period between operations.
    /// </summary>
    /// <param name="newPeriod">The new period between tasks</param>
    protected void ChangePeriod(TimeSpan newPeriod) => _period = newPeriod;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _timer?.Dispose();
            }

            _disposedValue = true;
        }
    }
}
