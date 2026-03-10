// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before media is deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation.
///     The notification is published by the <see cref="Services.IMediaService"/> when DeleteMediaOfType, Delete, or EmptyRecycleBin methods are called.
/// </remarks>
public sealed class MediaDeletingNotification : DeletingNotification<IMedia>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaDeletingNotification"/> class with a single media item.
    /// </summary>
    /// <param name="target">The media item being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaDeletingNotification(IMedia target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaDeletingNotification"/> class with multiple media items.
    /// </summary>
    /// <param name="target">The collection of media items being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaDeletingNotification(IEnumerable<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
