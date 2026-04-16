using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
/// Base class for recurring background jobs that provides default values for common properties.
/// </summary>
/// <remarks>
/// Implementors only need to provide <see cref="Period" /> and <see cref="RunJobAsync(CancellationToken)" />.
/// </remarks>
public abstract class RecurringBackgroundJobBase : IRecurringBackgroundJob
{
    /// <summary>
    /// The default delay to use for recurring tasks for the first run after application start-up if no alternative is configured.
    /// </summary>
    /// <remarks>
    /// The default of 3 minutes is chosen to allow the application to finish starting up and stabilize before the first execution of recurring tasks.
    /// </remarks>
    protected internal static readonly TimeSpan DefaultDelay = TimeSpan.FromMinutes(3);

    /// <summary>
    /// The default server roles that recurring background jobs run on.
    /// </summary>
    /// <remarks>
    /// The default of running on both <see cref="ServerRole.Single" /> and <see cref="ServerRole.SchedulingPublisher" /> is chosen to ensure recurring background jobs do not run on every server (in a load-balanced environment).
    /// </remarks>
    protected internal static readonly ServerRole[] DefaultServerRoles = [ServerRole.Single, ServerRole.SchedulingPublisher];

    /// <inheritdoc />
    public abstract TimeSpan Period { get; }

    /// <inheritdoc />
    public virtual TimeSpan Delay => DefaultDelay;

    /// <inheritdoc />
    public virtual ServerRole[] ServerRoles => DefaultServerRoles;

    /// <inheritdoc />
    public virtual event EventHandler PeriodChanged { add { } remove { } }

    /// <inheritdoc />
    [Obsolete("Use RunJobAsync(CancellationToken) instead. Scheduled for removal in Umbraco 19.")]
    public Task RunJobAsync() => RunJobAsync(CancellationToken.None);

    /// <inheritdoc />
    public abstract Task RunJobAsync(CancellationToken cancellationToken);
}
