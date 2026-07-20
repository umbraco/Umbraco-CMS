using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Search.Core.Cache.Index;

internal sealed class RebuildIndexCacheRefresherNotification : CacheRefresherNotification
{
    public RebuildIndexCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
