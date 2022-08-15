using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

public class ContentTypeCacheRefresherNotification : CacheRefresherNotification
{
    public ContentTypeCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(
        messageObject,
        messageType)
    {
    }
}
