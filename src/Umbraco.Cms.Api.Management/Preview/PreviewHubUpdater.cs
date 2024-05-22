using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Api.Management.Preview;

public class PreviewHubUpdater : INotificationAsyncHandler<ContentCacheRefresherNotification>
{
    private readonly Lazy<IHubContext<PreviewHub, IPreviewHub>> _hubContext;
    private readonly IRuntimeState _runtimeState;

    // using a lazy arg here means that we won't create the hub until necessary
    // and therefore we won't have too bad an impact on boot time
    public PreviewHubUpdater(
        Lazy<IHubContext<PreviewHub, IPreviewHub>> hubContext,
        IRuntimeState runtimeState)
    {
        _hubContext = hubContext;
        _runtimeState = runtimeState;
    }

    public async Task HandleAsync(ContentCacheRefresherNotification args, CancellationToken cancellationToken)
    {
        if (_runtimeState.Level is not RuntimeLevel.Run
            || args.MessageType is not MessageType.RefreshByPayload)
        {
            return;
        }

        var payloads = (ContentCacheRefresher.JsonPayload[])args.MessageObject;
        IHubContext<PreviewHub, IPreviewHub> hubContextInstance = _hubContext.Value;
        foreach (ContentCacheRefresher.JsonPayload payload in payloads)
        {
            if (payload.ChangeTypes is TreeChangeTypes.RefreshAll)
            {
                // pre v14, the payload was checked on the Id (int) resulting in a 0 on RefreshAll, the client on the other hand did not handle this
                // => ignore refreshAll
                continue;
            }

            var key = payload.Key; // keep it simple for now, ignore ChangeTypes
            if (key.HasValue is false)
            {
                throw new InvalidOperationException($"No key is set for payload with id {payload.Id}");
            }

            await hubContextInstance.Clients.All.refreshed(key.Value);
        }
    }
}
