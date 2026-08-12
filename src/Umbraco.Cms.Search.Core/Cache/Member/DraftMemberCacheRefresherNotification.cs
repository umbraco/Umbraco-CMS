using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Search.Core.Cache.Member;

/// <summary>
/// The distributed notification broadcast by <see cref="DraftMemberCacheRefresher"/>.
/// </summary>
internal sealed class DraftMemberCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DraftMemberCacheRefresherNotification"/> class.
    /// </summary>
    public DraftMemberCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
