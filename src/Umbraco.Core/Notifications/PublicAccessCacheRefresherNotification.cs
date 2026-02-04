// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when public access cache needs to be refreshed.
/// </summary>
/// <remarks>
///     This notification is used to synchronize public access cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class PublicAccessCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PublicAccessCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about the public access entries to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public PublicAccessCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
