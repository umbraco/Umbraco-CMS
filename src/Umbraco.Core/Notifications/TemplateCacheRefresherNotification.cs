using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

public class TemplateCacheRefresherNotification : CacheRefresherNotification
{
    public TemplateCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
