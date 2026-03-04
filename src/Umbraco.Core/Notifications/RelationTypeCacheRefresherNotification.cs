// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when relation type cache needs to be refreshed.
/// </summary>
/// <remarks>
///     This notification is used to synchronize relation type cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class RelationTypeCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationTypeCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about the relation types to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public RelationTypeCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
