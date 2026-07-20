using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Search.Core.Cache.PublicAccess;

internal sealed class PublicAccessDetailedCacheRefresherNotification : CacheRefresherNotification
{
    public PublicAccessDetailedCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
