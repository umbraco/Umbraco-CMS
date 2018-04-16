using System;
using LightInject;
using Microsoft.AspNet.SignalR;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;

namespace Umbraco.Web.SignalR
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class PreviewHubComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);
            composition.Container.Register(_ => GlobalHost.ConnectionManager.GetHubContext<PreviewHub, IPreviewHub>(), new PerContainerLifetime());
        }

        // using a lazy arg here means that we won't create the hub until necessary
        // and therefore we won't have too bad an impact on boot time
        public void Initialize(Lazy<IHubContext<IPreviewHub>> hubContext)
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
                var payloads = (ContentCacheRefresher.JsonPayload[])args.MessageObject;
                var hubContextInstance = hubContext.Value;
                foreach (var payload in payloads)
                {
                    var id = payload.Id; // keep it simple for now, ignore ChangeTypes
                    hubContextInstance.Clients.All.refreshed(id);
                }
            };
        }
    }
}
