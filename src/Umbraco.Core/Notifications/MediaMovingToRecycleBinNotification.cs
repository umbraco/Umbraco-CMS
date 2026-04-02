// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before media is moved to the recycle bin.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the move to recycle bin operation.
///     The notification is published by the <see cref="Services.IMediaService"/> when MoveToRecycleBin is called.
/// </remarks>
public sealed class MediaMovingToRecycleBinNotification : MovingToRecycleBinNotification<IMedia>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaMovingToRecycleBinNotification"/> class with a single media item.
    /// </summary>
    /// <param name="target">The move to recycle bin information for the media item being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaMovingToRecycleBinNotification(MoveToRecycleBinEventInfo<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaMovingToRecycleBinNotification"/> class with multiple media items.
    /// </summary>
    /// <param name="target">The collection of move to recycle bin information for media items being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaMovingToRecycleBinNotification(IEnumerable<MoveToRecycleBinEventInfo<IMedia>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
