// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that triggers the member cache refresher.
/// </summary>
/// <remarks>
///     This notification is used to synchronize member cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class MemberCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about the member to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public MemberCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
