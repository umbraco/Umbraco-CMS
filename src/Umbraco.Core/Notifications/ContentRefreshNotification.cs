// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when content needs to be refreshed in the cache.
/// </summary>
/// <remarks>
///     This notification is used internally for cache synchronization purposes.
///     For normal content operations, use <see cref="ContentSavedNotification"/> or
///     <see cref="ContentPublishedNotification"/> instead.
/// </remarks>
[Obsolete("This is only used for the internal cache and will change, use saved notifications instead")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class ContentRefreshNotification : EntityRefreshNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentRefreshNotification"/> class.
    /// </summary>
    /// <param name="target">The content item to refresh.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentRefreshNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }
}
