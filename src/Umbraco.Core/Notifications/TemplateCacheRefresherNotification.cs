// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when template cache needs to be refreshed.
/// </summary>
/// <remarks>
///     This notification is used to synchronize template cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class TemplateCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about the templates to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public TemplateCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
