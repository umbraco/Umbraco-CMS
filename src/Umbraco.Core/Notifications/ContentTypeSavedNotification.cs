// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a content type has been saved.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IContentTypeService"/> after the content type has been persisted.
///     It is not cancelable since the save operation has already completed.
/// </remarks>
public class ContentTypeSavedNotification : SavedNotification<IContentType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeSavedNotification"/> class with a single content type.
    /// </summary>
    /// <param name="target">The content type that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeSavedNotification(IContentType target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeSavedNotification"/> class with multiple content types.
    /// </summary>
    /// <param name="target">The collection of content types that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeSavedNotification(IEnumerable<IContentType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
