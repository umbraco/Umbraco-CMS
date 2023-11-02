using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public class WebhookBackgroundTaskQueue : IWebhookBackgroundTaskQueue
{
    private readonly ConcurrentQueue<Func<Task<WebhookResponseModel>>> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);

    public async Task QueueBackgroundWorkItemAsync(Func<Task<WebhookResponseModel>> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        _queue.Enqueue(workItem);
        _signal.Release();
    }

    public async Task<Func<Task<WebhookResponseModel>>?> DequeueAsync()
    {
        await _signal.WaitAsync();
        _queue.TryDequeue(out Func<Task<WebhookResponseModel>>? workItem);
        return workItem;
    }
}
