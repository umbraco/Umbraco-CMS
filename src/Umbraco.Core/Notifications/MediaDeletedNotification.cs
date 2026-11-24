// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after media has been deleted.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IMediaService"/> when Delete, DeleteMediaOfType or EmptyRecycleBin methods complete.
///     It is not cancelable since the delete operation has already completed.
///     Deleting a media item also deletes its descendants; each gets its own notification sharing the state of the
///     <see cref="MediaDeletingNotification"/> for the item chosen for deletion.
/// </remarks>
public sealed class MediaDeletedNotification : DeletedNotification<IMedia>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaDeletedNotification"/> class with a single media item.
    /// </summary>
    /// <param name="target">The media item that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaDeletedNotification(IMedia target, EventMessages messages)
        : base(target, messages)
    {
    }
}
