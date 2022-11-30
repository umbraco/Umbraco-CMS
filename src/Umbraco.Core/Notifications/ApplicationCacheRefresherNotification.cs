using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

public class ApplicationCacheRefresherNotification : CacheRefresherNotification
{
    public ApplicationCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(
        messageObject,
        messageType)
    {
    }
}
