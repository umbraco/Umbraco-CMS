using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

public class RelationTypeCacheRefresherNotification : CacheRefresherNotification
{
    public RelationTypeCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
