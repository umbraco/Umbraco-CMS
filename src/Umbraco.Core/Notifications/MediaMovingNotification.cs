// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before media is moved.
/// </summary>
/// <remarks>
///     <para>
///         This notification is cancelable, allowing handlers to prevent the move operation.
///         The notification is published by the <see cref="Services.IMediaService"/> before media is moved.
///     </para>
///     <para>
///         NOTE: If the target parent is the recycle bin, this notification is never published.
///         Use <see cref="MediaMovingToRecycleBinNotification"/> instead.
///     </para>
/// </remarks>
public sealed class MediaMovingNotification : MovingNotification<IMedia>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaMovingNotification"/> class with a single media item.
    /// </summary>
    /// <param name="target">The move information for the media item being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaMovingNotification(MoveEventInfo<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaMovingNotification"/> class with multiple media items.
    /// </summary>
    /// <param name="target">The collection of move information for media items being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaMovingNotification(IEnumerable<MoveEventInfo<IMedia>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
