// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before content is unpublished.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the unpublish operation.
///     The notification is published by the <see cref="Services.IContentService"/> before content is removed from the public site.
/// </remarks>
public sealed class ContentUnpublishingNotification : CancelableEnumerableObjectNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentUnpublishingNotification"/> class with a single content item.
    /// </summary>
    /// <param name="target">The content item being unpublished.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentUnpublishingNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentUnpublishingNotification"/> class with multiple content items.
    /// </summary>
    /// <param name="target">The collection of content items being unpublished.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentUnpublishingNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the collection of <see cref="IContent"/> items being unpublished.
    /// </summary>
    public IEnumerable<IContent> UnpublishedEntities => Target;
}
