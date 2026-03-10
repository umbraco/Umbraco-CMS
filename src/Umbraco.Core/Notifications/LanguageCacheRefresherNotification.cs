// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when language cache needs to be refreshed.
/// </summary>
/// <remarks>
///     This notification is used to synchronize language cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class LanguageCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LanguageCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about the languages to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public LanguageCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
