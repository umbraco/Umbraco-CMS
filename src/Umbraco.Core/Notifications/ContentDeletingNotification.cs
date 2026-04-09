// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before content is deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation.
///     The notification is published by the <see cref="Services.IContentService"/> when DeleteContentOfType, Delete, or EmptyRecycleBin methods are called.
/// </remarks>
public sealed class ContentDeletingNotification : DeletingNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentDeletingNotification"/> class with a single content item.
    /// </summary>
    /// <param name="target">The content item being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentDeletingNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentDeletingNotification"/> class with multiple content items.
    /// </summary>
    /// <param name="target">The collection of content items being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentDeletingNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
