namespace Umbraco.Cms.Infrastructure.HostedServices;

/// <summary>
///     A Background Task Queue, to enqueue tasks for executing in the background.
/// </summary>
/// <remarks>
///     Borrowed from https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-5.0
/// </remarks>
public interface IBackgroundTaskQueue
{
    /// <summary>
    ///     Enqueue a work item to be executed on in the background.
    /// </summary>
    void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

    /// <summary>
    ///     Dequeue the first item on the queue.
    /// </summary>
    Task<Func<CancellationToken, Task>?> DequeueAsync(CancellationToken cancellationToken);
}
