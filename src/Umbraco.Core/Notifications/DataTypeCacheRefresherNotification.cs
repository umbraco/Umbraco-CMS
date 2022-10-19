using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

public class DataTypeCacheRefresherNotification : CacheRefresherNotification
{
    public DataTypeCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
