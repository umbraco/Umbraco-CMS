using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache
{
    public class RelationTypeCacheRefresherNotification : CacheRefresherNotification
    {
        public RelationTypeCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType)
        {
        }
    }
}
