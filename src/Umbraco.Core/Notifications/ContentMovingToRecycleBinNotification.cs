// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before content is moved to the recycle bin.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the move to recycle bin operation.
///     The notification is published by the <see cref="Services.IContentService"/> when MoveToRecycleBin is called.
/// </remarks>
public sealed class ContentMovingToRecycleBinNotification : MovingToRecycleBinNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentMovingToRecycleBinNotification"/> class with a single content item.
    /// </summary>
    /// <param name="target">The move to recycle bin information for the content item being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentMovingToRecycleBinNotification(MoveToRecycleBinEventInfo<IContent> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentMovingToRecycleBinNotification"/> class with multiple content items.
    /// </summary>
    /// <param name="target">The collection of move to recycle bin information for content items being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentMovingToRecycleBinNotification(IEnumerable<MoveToRecycleBinEventInfo<IContent>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
