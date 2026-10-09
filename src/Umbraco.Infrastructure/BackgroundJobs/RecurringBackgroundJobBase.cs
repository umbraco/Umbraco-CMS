using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
/// Base class for recurring background jobs that provides default values for common properties.
/// </summary>
/// <remarks>
/// Implementors must pass an initial <see cref="Period" /> to the base constructor and implement <see cref="RunJobAsync(CancellationToken)" />.
/// </remarks>
public abstract class RecurringBackgroundJobBase : IRecurringBackgroundJob, IDisposable
{
    /// <summary>
    /// The default delay to use for recurring tasks for the first run after application start-up if no alternative is configured.
    /// </summary>
    /// <remarks>
    /// The default of 3 minutes is chosen to allow the application to finish starting up and stabilize before the first execution of recurring tasks.
    /// </remarks>
    protected internal static readonly TimeSpan DefaultDelay = TimeSpan.FromMinutes(3);

    /// <summary>
    /// The default back-off to use when an execution is ignored, before re-evaluating execution conditions.
    /// </summary>
    /// <remarks>
    /// The default of 1 minute prevents tight looping when an execution is skipped (e.g. runtime not ready, wrong server role or not main domain) and the configured <see cref="IRecurringBackgroundJob.Period" /> is short or <see cref="TimeSpan.Zero" />.
    /// </remarks>
    protected internal static readonly TimeSpan DefaultIgnoredDelay = TimeSpan.FromMinutes(1);

    /// <summary>
    /// The default server roles that recurring background jobs run on.
    /// </summary>
    /// <remarks>
    /// The default of running on both <see cref="ServerRole.Single" /> and <see cref="ServerRole.SchedulingPublisher" /> is chosen to ensure recurring background jobs do not run on every server (in a load-balanced environment).
    /// </remarks>
    protected internal static readonly ServerRole[] DefaultServerRoles = [ServerRole.Single, ServerRole.SchedulingPublisher];

    private TimeSpan _period;
    private TimeSpan _ignoredDelay = DefaultIgnoredDelay;
    private EventHandler? _periodChanged;
    private EventHandler? _ignoredDelayChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecurringBackgroundJobBase" /> class with the specified initial <paramref name="period" />. The initial value is stored directly without raising <see cref="PeriodChanged" />.
    /// </summary>
    /// <param name="period">The initial period between executions. Set to <see cref="Timeout.InfiniteTimeSpan" /> for a manual-trigger-only job.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period" /> is negative and not <see cref="Timeout.InfiniteTimeSpan" />.</exception>
    protected RecurringBackgroundJobBase(TimeSpan period)
    {
        if (period != Timeout.InfiniteTimeSpan)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(period, TimeSpan.Zero);
        }

        _period = period;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Setting this property to a different value raises <see cref="PeriodChanged" />. The initial value passed to the constructor is stored without raising the event.
    /// </remarks>
    public virtual TimeSpan Period
    {
        get => _period;
        protected set
        {
            if (value != Timeout.InfiniteTimeSpan)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, TimeSpan.Zero);
            }

            if (_period == value)
            {
                return;
            }

            _period = value;
            OnPeriodChanged(EventArgs.Empty);
        }
    }

    /// <inheritdoc />
    public virtual TimeSpan Delay => DefaultDelay;

    /// <inheritdoc />
    /// <remarks>
    /// Setting this property to a different value raises <see cref="IgnoredDelayChanged" />. The initial value (<see cref="DefaultIgnoredDelay" />) is set without raising the event.
    /// </remarks>
    public virtual TimeSpan IgnoredDelay
    {
        get => _ignoredDelay;
        protected set
        {
            if (value != Timeout.InfiniteTimeSpan)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, TimeSpan.Zero);
            }

            if (_ignoredDelay == value)
            {
                return;
            }

            _ignoredDelay = value;
            OnIgnoredDelayChanged(EventArgs.Empty);
        }
    }

    /// <inheritdoc />
    public virtual ServerRole[] ServerRoles => DefaultServerRoles;

    /// <inheritdoc />
    public virtual event EventHandler PeriodChanged
    {
        add { _periodChanged += value; }
        remove { _periodChanged -= value; }
    }

    /// <inheritdoc />
    public virtual event EventHandler IgnoredDelayChanged
    {
        add { _ignoredDelayChanged += value; }
        remove { _ignoredDelayChanged -= value; }
    }

    /// <summary>
    /// Raises the <see cref="PeriodChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    /// <remarks>
    /// Override this when overriding <see cref="PeriodChanged" /> to dispatch through the overridden event's backing delegate.
    /// </remarks>
    protected virtual void OnPeriodChanged(EventArgs e)
        => _periodChanged?.Invoke(this, e);

    /// <summary>
    /// Raises the <see cref="IgnoredDelayChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    /// <remarks>
    /// Override this when overriding <see cref="IgnoredDelayChanged" /> to dispatch through the overridden event's backing delegate.
    /// </remarks>
    protected virtual void OnIgnoredDelayChanged(EventArgs e)
        => _ignoredDelayChanged?.Invoke(this, e);

    /// <inheritdoc />
    [Obsolete("Use RunJobAsync(CancellationToken) instead. Scheduled for removal in Umbraco 19.")]
    public Task RunJobAsync() => RunJobAsync(CancellationToken.None);

    /// <inheritdoc />
    public abstract Task RunJobAsync(CancellationToken cancellationToken);

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the resources used by this job. Subclasses adding disposable state should override this method, dispose their own resources, and call <c>base.Dispose(disposing)</c>.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clear the subscriber delegates so the job does not retain references to (or invoke) listeners after disposal.
            _periodChanged = null;
            _ignoredDelayChanged = null;
        }
    }
}
