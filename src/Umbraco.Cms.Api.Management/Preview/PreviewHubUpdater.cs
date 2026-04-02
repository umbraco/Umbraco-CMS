using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Preview;

/// <summary>
/// Provides methods for updating and managing the state of the preview hub within the Umbraco CMS API management layer.
/// Typically used to refresh or synchronize preview content for clients connected to the preview hub.
/// </summary>
public class PreviewHubUpdater : INotificationAsyncHandler<ContentCacheRefresherNotification>
{
    private readonly Lazy<IHubContext<PreviewHub, IPreviewHub>> _hubContext;

    // using a lazy arg here means that we won't create the hub until necessary
    // and therefore we won't have too bad an impact on boot time
    /// <summary>
    /// Initializes a new instance of the <see cref="PreviewHubUpdater"/> class.
    /// </summary>
    /// <param name="hubContext">
    /// A lazily-initialized <see cref="IHubContext{T, TClient}"/> for the <see cref="PreviewHub"/>, used to manage real-time preview updates.
    /// </param>
    public PreviewHubUpdater(Lazy<IHubContext<PreviewHub, IPreviewHub>> hubContext) => _hubContext = hubContext;


    /// <summary>
    /// Asynchronously handles a <see cref="ContentCacheRefresherNotification"/> to update preview clients when content cache changes occur.
    /// Processes notifications of type <c>RefreshByPayload</c> and notifies all connected preview clients if relevant content nodes are refreshed.
    /// </summary>
    /// <param name="args">The notification containing the message type and payloads describing content cache changes.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleAsync(ContentCacheRefresherNotification args, CancellationToken cancellationToken)
    {
        if (args.MessageType != MessageType.RefreshByPayload)
        {
            return;
        }

        var payloads = (ContentCacheRefresher.JsonPayload[])args.MessageObject;
        IHubContext<PreviewHub, IPreviewHub> hubContextInstance = _hubContext.Value;
        foreach (ContentCacheRefresher.JsonPayload payload in payloads)
        {
            if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshNode) is false)
            {
                continue;
            }

            if (payload.Key.HasValue is false)
            {
                throw new InvalidOperationException($"No key is set for payload with id {payload.Id}");
            }

            await hubContextInstance.Clients.All.refreshed(payload.Key.Value);
        }
    }
}
