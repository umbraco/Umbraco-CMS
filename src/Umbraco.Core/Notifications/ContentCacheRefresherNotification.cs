using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

public class ContentCacheRefresherNotification : CacheRefresherNotification
{
    public ContentCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(
        messageObject,
        messageType)
    {
    }
}
