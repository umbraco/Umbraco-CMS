// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before a content type is deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation.
///     The notification is published by the <see cref="Services.IContentTypeService"/> before the content type is removed.
/// </remarks>
public class ContentTypeDeletingNotification : DeletingNotification<IContentType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeDeletingNotification"/> class with a single content type.
    /// </summary>
    /// <param name="target">The content type being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeDeletingNotification(IContentType target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeDeletingNotification"/> class with multiple content types.
    /// </summary>
    /// <param name="target">The collection of content types being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeDeletingNotification(IEnumerable<IContentType> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }
}
