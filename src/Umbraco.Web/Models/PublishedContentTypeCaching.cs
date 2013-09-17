using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;

namespace Umbraco.Web.Models
{
    // note
    // this is probably how we should refresh the Core.Models.PublishedContentType cache, by subscribing to
    // events from the ContentTypeCacheRefresher - however as of may 1st, 2013 that eventing system is not
    // fully operational and Shannon prefers that the refresh code is hard-wired into the refresher. so this
    // is commented out and the refresher calls PublishedContentType.Clear() directly.
    // TODO refactor this when the refresher is ready
    // FIXME should use the right syntax NOW

    class PublishedContentTypeCaching : ApplicationEventHandler
    {
        protected override void ApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentTypeCacheRefresher.CacheUpdated += ContentTypeCacheUpdated;
            DataTypeCacheRefresher.CacheUpdated += DataTypeCacheUpdated;
            base.ApplicationInitialized(umbracoApplication, applicationContext);
        }

        private static void ContentTypeCacheUpdated(ContentTypeCacheRefresher sender, CacheRefresherEventArgs e)
        {
            switch (e.MessageType)
            {
                case MessageType.RefreshAll:
                    PublishedContentType.ClearAll();
                    break;
                case MessageType.RefreshById:
                case MessageType.RemoveById:
                    PublishedContentType.ClearContentType((int)e.MessageObject);
                    break;
                case MessageType.RefreshByInstance:
                case MessageType.RemoveByInstance:
                    PublishedContentType.ClearContentType(((IContentType)e.MessageObject).Id);
                    break;
                case MessageType.RefreshByJson:
                    var jsonPayload = (string)e.MessageObject;
                    // TODO ?? FUCK! this is what we get now what?
                    break;
                default:
                    throw new ArgumentOutOfRangeException("e", "Unknown message type.");
            }
        }

        private static void DataTypeCacheUpdated(DataTypeCacheRefresher sender, CacheRefresherEventArgs e)
        {
            switch (e.MessageType)
            {
                case MessageType.RefreshAll:
                    PublishedContentType.ClearAll();
                    break;
                case MessageType.RefreshById:
                case MessageType.RemoveById:
                    PublishedContentType.ClearDataType((int)e.MessageObject);
                    break;
                case MessageType.RefreshByInstance:
                case MessageType.RemoveByInstance:
                    PublishedContentType.ClearDataType(((IDataTypeDefinition)e.MessageObject).Id);
                    break;
                case MessageType.RefreshByJson:
                    var jsonPayload = (string)e.MessageObject;
                    // TODO ??
                    break;
                default:
                    throw new ArgumentOutOfRangeException("e", "Unknown message type.");
            }
        }
    }
}
