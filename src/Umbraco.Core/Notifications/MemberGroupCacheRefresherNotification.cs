// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when member group cache needs to be refreshed.
/// </summary>
/// <remarks>
///     This notification is used to synchronize member group cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class MemberGroupCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberGroupCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about the member groups to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public MemberGroupCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
