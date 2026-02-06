// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when media needs to be refreshed in the cache.
/// </summary>
/// <remarks>
///     This notification is used internally for cache synchronization purposes.
///     For normal media operations, use <see cref="MediaTreeChangeNotification"/> instead.
/// </remarks>
[Obsolete("This is only used for the internal cache and will change, use tree change notifications instead")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class MediaRefreshNotification : EntityRefreshNotification<IMedia>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaRefreshNotification"/> class.
    /// </summary>
    /// <param name="target">The media item to refresh.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaRefreshNotification(IMedia target, EventMessages messages)
        : base(target, messages)
    {
    }
}
