// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after content has been moved.
/// </summary>
/// <remarks>
///     <para>
///         This notification is published by the <see cref="Services.IContentService"/> after content has been moved.
///         It is not cancelable since the move operation has already completed.
///     </para>
///     <para>
///         NOTE: If the target parent is the recycle bin, this notification is never published.
///         Use <see cref="ContentMovedToRecycleBinNotification"/> instead.
///     </para>
/// </remarks>
public sealed class ContentMovedNotification : MovedNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentMovedNotification"/> class with a single content item.
    /// </summary>
    /// <param name="target">The move information for the content item that was moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentMovedNotification(MoveEventInfo<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentMovedNotification"/> class with multiple content items.
    /// </summary>
    /// <param name="target">The collection of move information for content items that were moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentMovedNotification(IEnumerable<MoveEventInfo<IContent>> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }
}
