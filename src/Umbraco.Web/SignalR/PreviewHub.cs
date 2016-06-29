using Microsoft.AspNet.SignalR;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;

namespace Umbraco.Web.SignalR
{
    public class PreviewHub : Hub
    {
        internal static void Initialize(IHubContext hubContext)
        {
            // ContentService.Saved is too soon - the content cache is not ready yet
            // try using the content cache refresher event, because when it triggers
            // the cache has already been notified of the changes
            //ContentService.Saved += (sender, args) =>
            //{
            //    var entity = args.SavedEntities.FirstOrDefault();
            //    if (entity != null)
            //        _previewHub.Clients.All.refreshed(entity.Id);
            //};

            ContentCacheRefresher.CacheUpdated += (sender, args) =>
            {
                if (args.MessageType != MessageType.RefreshByPayload) return;
                var payloads = (ContentCacheRefresher.JsonPayload[]) args.MessageObject;
                foreach (var payload in payloads)
                {
                    var id = payload.Id; // keep it simple for now, ignore ChangeTypes
                    hubContext.Clients.All.refreshed(id);
                }
            };
        }
    }
}
