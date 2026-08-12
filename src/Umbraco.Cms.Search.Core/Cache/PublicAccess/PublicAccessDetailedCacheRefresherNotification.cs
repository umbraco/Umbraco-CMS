using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Search.Core.Cache.PublicAccess;

/// <summary>
/// The distributed notification broadcast by <see cref="PublicAccessDetailedCacheRefresher"/>.
/// </summary>
internal sealed class PublicAccessDetailedCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublicAccessDetailedCacheRefresherNotification"/> class.
    /// </summary>
    public PublicAccessDetailedCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
