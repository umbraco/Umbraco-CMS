// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after the content recycle bin has been emptied.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IContentService"/> after the recycle bin has been emptied.
///     It is not cancelable since the operation has already completed.
/// </remarks>
public sealed class ContentEmptiedRecycleBinNotification : EmptiedRecycleBinNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentEmptiedRecycleBinNotification"/> class.
    /// </summary>
    /// <param name="deletedEntities">The collection of content items that were permanently deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentEmptiedRecycleBinNotification(IEnumerable<IContent> deletedEntities, EventMessages messages)
        : base(
        deletedEntities, messages)
    {
    }
}
