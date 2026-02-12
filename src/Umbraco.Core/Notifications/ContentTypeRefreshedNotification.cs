// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when content types have been refreshed in the cache.
/// </summary>
/// <remarks>
///     This notification is used internally for cache synchronization purposes.
///     For normal content type operations, use <see cref="ContentTypeSavedNotification"/> instead.
/// </remarks>
[Obsolete("This is only used for the internal cache and will change, use saved notifications instead")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class ContentTypeRefreshedNotification : ContentTypeRefreshNotification<IContentType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeRefreshedNotification"/> class
    ///     with a single content type change.
    /// </summary>
    /// <param name="target">The content type change that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeRefreshedNotification(ContentTypeChange<IContentType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeRefreshedNotification"/> class
    ///     with multiple content type changes.
    /// </summary>
    /// <param name="target">The collection of content type changes that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeRefreshedNotification(IEnumerable<ContentTypeChange<IContentType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
