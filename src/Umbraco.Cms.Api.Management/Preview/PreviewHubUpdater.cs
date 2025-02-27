using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Preview;

public class PreviewHubUpdater : INotificationAsyncHandler<ContentCacheRefresherNotification>
{
    private readonly Lazy<IHubContext<PreviewHub, IPreviewHub>> _hubContext;

    // using a lazy arg here means that we won't create the hub until necessary
    // and therefore we won't have too bad an impact on boot time
    public PreviewHubUpdater(Lazy<IHubContext<PreviewHub, IPreviewHub>> hubContext) => _hubContext = hubContext;


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
