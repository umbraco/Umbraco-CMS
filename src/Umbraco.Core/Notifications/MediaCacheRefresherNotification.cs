using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

public class MediaCacheRefresherNotification : CacheRefresherNotification
{
    public MediaCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
