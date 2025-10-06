namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
/// A background job that will be executed by an available server. With a single server setup this will always be the same.
/// With a load balanced setup, the executing server might change every time this needs to be executed.
/// </summary>
public interface IDistributedBackgroundJob
{
    /// <summary>
    /// Name of the job
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Timespan representing how often the task should recur.
    /// </summary>
    TimeSpan Period { get; }

    /// <summary>
    /// Run the job.
    /// </summary>
    /// <returns></returns>
    Task RunJobAsync();
}
