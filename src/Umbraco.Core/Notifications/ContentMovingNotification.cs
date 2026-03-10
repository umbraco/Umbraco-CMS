// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before content is moved.
/// </summary>
/// <remarks>
///     <para>
///         This notification is cancelable, allowing handlers to prevent the move operation.
///         The notification is published by the <see cref="Services.IContentService"/> before content is moved.
///     </para>
///     <para>
///         NOTE: If the target parent is the recycle bin, this notification is never published.
///         Use <see cref="ContentMovingToRecycleBinNotification"/> instead.
///     </para>
/// </remarks>
public sealed class ContentMovingNotification : MovingNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentMovingNotification"/> class with a single content item.
    /// </summary>
    /// <param name="target">The move information for the content item being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentMovingNotification(MoveEventInfo<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentMovingNotification"/> class with multiple content items.
    /// </summary>
    /// <param name="target">The collection of move information for content items being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentMovingNotification(IEnumerable<MoveEventInfo<IContent>> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }
}
