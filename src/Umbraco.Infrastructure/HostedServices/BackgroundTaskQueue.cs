using System.Collections.Concurrent;

namespace Umbraco.Cms.Infrastructure.HostedServices;

/// <summary>
///     A Background Task Queue, to enqueue tasks for executing in the background.
/// </summary>
/// <remarks>
///     Borrowed from https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-5.0
/// </remarks>
public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly SemaphoreSlim _signal = new(0);

    private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new();

    /// <inheritdoc />
    public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        _workItems.Enqueue(workItem);
        _signal.Release();
    }

    /// <inheritdoc />
    public async Task<Func<CancellationToken, Task>?> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _workItems.TryDequeue(out Func<CancellationToken, Task>? workItem);

        return workItem;
    }
}
