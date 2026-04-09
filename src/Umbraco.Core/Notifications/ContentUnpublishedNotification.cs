// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after content has been unpublished.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IContentService"/> after content has been removed from the public site.
///     It is not cancelable since the unpublish operation has already completed.
/// </remarks>
public sealed class ContentUnpublishedNotification : EnumerableObjectNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentUnpublishedNotification"/> class with a single content item.
    /// </summary>
    /// <param name="target">The content item that was unpublished.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentUnpublishedNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentUnpublishedNotification"/> class with multiple content items.
    /// </summary>
    /// <param name="target">The collection of content items that were unpublished.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentUnpublishedNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the collection of <see cref="IContent"/> items that were unpublished.
    /// </summary>
    public IEnumerable<IContent> UnpublishedEntities => Target;
}
