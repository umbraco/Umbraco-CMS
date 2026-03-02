// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before content is published.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the publish operation.
///     The notification is published by the <see cref="Services.IContentService"/> before content is made publicly available.
/// </remarks>
public sealed class ContentPublishingNotification : CancelableEnumerableObjectNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPublishingNotification"/> class with a single content item.
    /// </summary>
    /// <param name="target">The content item being published.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentPublishingNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPublishingNotification"/> class with multiple content items.
    /// </summary>
    /// <param name="target">The collection of content items being published.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentPublishingNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the collection of <see cref="IContent"/> items being published.
    /// </summary>
    public IEnumerable<IContent> PublishedEntities => Target;
}
