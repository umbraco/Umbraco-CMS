// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that triggers the external member cache refresher.
/// </summary>
/// <remarks>
///     This notification is used to synchronize external member cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class ExternalMemberCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalMemberCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about the external member to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public ExternalMemberCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
