using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache
{
    public class UserGroupCacheRefresherNotification : CacheRefresherNotification
    {
        public UserGroupCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType)
        {
        }
    }
}
