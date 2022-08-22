using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

public class UserGroupCacheRefresherNotification : CacheRefresherNotification
{
    public UserGroupCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
