// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after content has been saved.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IContentService"/> after content has been persisted.
///     It is not cancelable since the save operation has already completed.
/// </remarks>
public sealed class ContentSavedNotification : SavedNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSavedNotification"/> class with a single content item.
    /// </summary>
    /// <param name="target">The content item that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentSavedNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSavedNotification"/> class with multiple content items.
    /// </summary>
    /// <param name="target">The collection of content items that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentSavedNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
