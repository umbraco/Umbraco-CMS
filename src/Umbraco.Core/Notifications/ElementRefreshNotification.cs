// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when an element needs to be refreshed in the cache.
/// </summary>
/// <remarks>
///     This notification is used internally for cache synchronization purposes.
///     For normal element operations, use <see cref="ElementSavedNotification"/> or
///     <see cref="ElementPublishedNotification"/> instead.
/// </remarks>
[Obsolete("This is only used for the internal cache and will change, use saved notifications instead")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class ElementRefreshNotification : EntityRefreshNotification<IElement>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementRefreshNotification"/> class.
    /// </summary>
    /// <param name="target">The element item to refresh.</param>
    /// <param name="messages">The event messages collection.</param>
    public ElementRefreshNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }
}
