// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a content type has been deleted.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IContentTypeService"/> after the content type has been removed.
///     It is not cancelable since the delete operation has already completed.
/// </remarks>
public class ContentTypeDeletedNotification : DeletedNotification<IContentType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeDeletedNotification"/> class with a single content type.
    /// </summary>
    /// <param name="target">The content type that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeDeletedNotification(IContentType target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeDeletedNotification"/> class with multiple content types.
    /// </summary>
    /// <param name="target">The collection of content types that were deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeDeletedNotification(IEnumerable<IContentType> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }
}
