// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after content has been published.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IContentService"/> after content has been made publicly available.
///     It is not cancelable since the publish operation has already completed.
/// </remarks>
public sealed class ContentPublishedNotification : EnumerableObjectNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPublishedNotification"/> class with a single content item.
    /// </summary>
    /// <param name="target">The content item that was published.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentPublishedNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPublishedNotification"/> class with multiple content items.
    /// </summary>
    /// <param name="target">The collection of content items that were published.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentPublishedNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPublishedNotification"/> class with multiple content items and descendant flag.
    /// </summary>
    /// <param name="target">The collection of content items that were published.</param>
    /// <param name="messages">The event messages collection.</param>
    /// <param name="includeDescendants">A value indicating whether descendants were included in the publish operation.</param>
    public ContentPublishedNotification(IEnumerable<IContent> target, EventMessages messages, bool includeDescendants)
        : base(target, messages) => IncludeDescendants = includeDescendants;

    /// <summary>
    ///     Gets the collection of <see cref="IContent"/> items that were published.
    /// </summary>
    public IEnumerable<IContent> PublishedEntities => Target;

    /// <summary>
    ///     Gets a value indicating whether descendants were included in the publish operation.
    /// </summary>
    public bool IncludeDescendants { get; }
}
