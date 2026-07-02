namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
/// A background job that will be executed by an available server. With a single server setup this will always be the same.
/// With a load balanced setup, the executing server might change every time this needs to be executed.
/// </summary>
public interface IDistributedBackgroundJob
{
    /// <summary>
    /// Name of the job.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Timespan representing how often the task should recur.
    /// </summary>
    TimeSpan Period { get; }

    /// <summary>
    /// Gets a value indicating whether the job's runs should be aligned to clock boundaries derived from <see cref="Period" />.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, the job becomes runnable on the next clock boundary that is a multiple of <see cref="Period" />
    /// (measured from a fixed <strong>UTC</strong> origin, so boundaries fall on round clock times such as on the minute
    /// or every N seconds) rather than at <c>LastRun + Period</c>.
    /// For predictable boundaries <see cref="Period" /> should divide evenly into one hour.
    /// The scheduler may cache this value when it first evaluates registered jobs; changing it at runtime may require an application restart.
    /// Defaults to <c>false</c>, preserving the original drift-from-completion behaviour.
    /// </remarks>
    bool AlignToClock => false;

    /// <summary>
    /// Run the job.
    /// </summary>
    Task ExecuteAsync();

    /// <summary>
    /// Run the job with a cancellation token that signals when the host is shutting down.
    /// </summary>
    /// <remarks>
    /// The default implementation delegates to <see cref="ExecuteAsync()"/>.
    /// Override this method to respond to graceful shutdown.
    /// </remarks>
    Task ExecuteAsync(CancellationToken cancellationToken) => ExecuteAsync();
}
