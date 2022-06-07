using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

public class MemberGroupCacheRefresherNotification : CacheRefresherNotification
{
    public MemberGroupCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
