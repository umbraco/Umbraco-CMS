// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after media has been moved.
/// </summary>
/// <remarks>
///     <para>
///         This notification is published by the <see cref="Services.IMediaService"/> after media has been moved.
///         It is not cancelable since the move operation has already completed.
///     </para>
///     <para>
///         NOTE: If the target parent is the recycle bin, this notification is never published.
///         Use <see cref="MediaMovedToRecycleBinNotification"/> instead.
///     </para>
/// </remarks>
public sealed class MediaMovedNotification : MovedNotification<IMedia>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaMovedNotification"/> class with a single media item.
    /// </summary>
    /// <param name="target">The move information for the media item that was moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaMovedNotification(MoveEventInfo<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaMovedNotification"/> class with multiple media items.
    /// </summary>
    /// <param name="target">The collection of move information for media items that were moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaMovedNotification(IEnumerable<MoveEventInfo<IMedia>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
