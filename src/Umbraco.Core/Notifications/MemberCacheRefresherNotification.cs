using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

public class MemberCacheRefresherNotification : CacheRefresherNotification
{
    public MemberCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
