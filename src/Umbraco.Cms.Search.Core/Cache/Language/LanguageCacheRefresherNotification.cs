using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Search.Core.Cache.Language;

/// <summary>
/// The distributed notification broadcast by <see cref="LanguageCacheRefresher"/>.
/// </summary>
internal sealed class LanguageCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageCacheRefresherNotification"/> class.
    /// </summary>
    public LanguageCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
