// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after content has been moved to the recycle bin.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IContentService"/> after content has been moved to the recycle bin.
///     It is not cancelable since the operation has already completed.
/// </remarks>
public sealed class ContentMovedToRecycleBinNotification : MovedToRecycleBinNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentMovedToRecycleBinNotification"/> class with a single content item.
    /// </summary>
    /// <param name="target">The move to recycle bin information for the content item that was moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentMovedToRecycleBinNotification(MoveToRecycleBinEventInfo<IContent> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentMovedToRecycleBinNotification"/> class with multiple content items.
    /// </summary>
    /// <param name="target">The collection of move to recycle bin information for content items that were moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentMovedToRecycleBinNotification(IEnumerable<MoveToRecycleBinEventInfo<IContent>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
