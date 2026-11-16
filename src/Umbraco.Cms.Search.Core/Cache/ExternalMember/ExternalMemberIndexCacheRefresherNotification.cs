using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Search.Core.Cache.ExternalMember;

/// <summary>
/// The distributed notification broadcast by <see cref="ExternalMemberIndexCacheRefresher"/>.
/// </summary>
internal sealed class ExternalMemberIndexCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalMemberIndexCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about what to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public ExternalMemberIndexCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
