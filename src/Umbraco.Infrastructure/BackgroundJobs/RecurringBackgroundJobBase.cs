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
    protected internal static readonly TimeSpan DefaultDelay = TimeSpan.FromMinutes(3);
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
