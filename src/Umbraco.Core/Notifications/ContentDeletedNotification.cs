// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after content has been deleted.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IContentService"/> when Delete, DeleteOfType or EmptyRecycleBin methods complete.
///     It is not cancelable since the delete operation has already completed.
///     Deleting a content item also deletes its descendants; each gets its own notification sharing the state of the
///     <see cref="ContentDeletingNotification"/> for the item chosen for deletion.
/// </remarks>
public sealed class ContentDeletedNotification : DeletedNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentDeletedNotification"/> class with a single content item.
    /// </summary>
    /// <param name="target">The content item that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentDeletedNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }
}
