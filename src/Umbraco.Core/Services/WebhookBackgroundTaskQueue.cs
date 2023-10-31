using System.Threading.Channels;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public class WebhookBackgroundTaskQueue : IWebhookBackgroundTaskQueue
{
    private readonly Channel<Func<Task<WebhookResponseModel>>> _queue;

    public WebhookBackgroundTaskQueue()
    {
        // Capacity should be set based on the expected application load and
        // number of concurrent threads accessing the queue.
        // BoundedChannelFullMode.Wait will cause calls to WriteAsync() to return a task,
        // which completes only when space became available. This leads to backpressure,
        // in case too many publishers/calls start accumulating.
        var options = new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
        };
        _queue = Channel.CreateBounded<Func<Task<WebhookResponseModel>>>(options);
    }

    public async Task QueueBackgroundWorkItemAsync(Func<Task<WebhookResponseModel>> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        await _queue.Writer.WriteAsync(workItem);
    }

    public async Task<Func<Task<WebhookResponseModel>>> DequeueAsync()
    {
        Func<Task<WebhookResponseModel>> workItem = await _queue.Reader.ReadAsync();

        return workItem;
    }
}
