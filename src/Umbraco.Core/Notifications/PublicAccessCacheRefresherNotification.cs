using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

public class PublicAccessCacheRefresherNotification : CacheRefresherNotification
{
    public PublicAccessCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
