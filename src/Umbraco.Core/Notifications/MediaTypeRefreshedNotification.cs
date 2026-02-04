// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when media types have been refreshed in the cache.
/// </summary>
/// <remarks>
///     This notification is used internally for cache synchronization purposes.
///     For normal media type operations, use tree change notifications instead.
/// </remarks>
[Obsolete("This is only used for the internal cache and will change, use tree change notifications instead")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class MediaTypeRefreshedNotification : ContentTypeRefreshNotification<IMediaType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTypeRefreshedNotification"/> class
    ///     with a single media type change.
    /// </summary>
    /// <param name="target">The media type change that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaTypeRefreshedNotification(ContentTypeChange<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTypeRefreshedNotification"/> class
    ///     with multiple media type changes.
    /// </summary>
    /// <param name="target">The collection of media type changes that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaTypeRefreshedNotification(IEnumerable<ContentTypeChange<IMediaType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
