using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Search.Core.Cache.Index;

/// <summary>
/// The distributed notification broadcast by <see cref="RebuildIndexCacheRefresher"/>.
/// </summary>
internal sealed class RebuildIndexCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildIndexCacheRefresherNotification"/> class.
    /// </summary>
    public RebuildIndexCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
