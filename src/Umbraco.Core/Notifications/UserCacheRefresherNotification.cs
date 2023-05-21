using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

public class UserCacheRefresherNotification : CacheRefresherNotification
{
    public UserCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
