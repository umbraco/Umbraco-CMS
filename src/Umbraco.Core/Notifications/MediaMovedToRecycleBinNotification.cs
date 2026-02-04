// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after media has been moved to the recycle bin.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IMediaService"/> after media has been moved to the recycle bin.
///     It is not cancelable since the operation has already completed.
/// </remarks>
public sealed class MediaMovedToRecycleBinNotification : MovedToRecycleBinNotification<IMedia>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaMovedToRecycleBinNotification"/> class with a single media item.
    /// </summary>
    /// <param name="target">The move to recycle bin information for the media item that was moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaMovedToRecycleBinNotification(MoveToRecycleBinEventInfo<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaMovedToRecycleBinNotification"/> class with multiple media items.
    /// </summary>
    /// <param name="target">The collection of move to recycle bin information for media items that were moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaMovedToRecycleBinNotification(IEnumerable<MoveToRecycleBinEventInfo<IMedia>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
