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
    ///     Initializes a new instance of the <see cref="ContentUnpublishedNotification"/> class with a single content item
    ///     and the cultures that were unpublished.
    /// </summary>
    /// <param name="target">The content item that was unpublished.</param>
    /// <param name="messages">The event messages collection.</param>
    /// <param name="unpublishedCultures">The cultures that were unpublished, keyed by content <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.</param>
    public ContentUnpublishedNotification(IContent target, EventMessages messages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? unpublishedCultures)
        : base(target, messages)
        => UnpublishedCultures = unpublishedCultures;

    /// <summary>
    ///     Gets the collection of <see cref="IContent"/> items that were unpublished.
    /// </summary>
    public IEnumerable<IContent> UnpublishedEntities => Target;

    /// <summary>
    ///     Gets the cultures that were unpublished for each content item, keyed by content
    ///     <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This notification is only raised when a document is unpublished <em>as a whole</em>; the value therefore
    ///         reports every culture that was taken down (for invariant content, <c>["*"]</c>). Unpublishing an individual
    ///         culture of a still-published variant document does not raise this notification — it is reported on
    ///         <see cref="ContentPublishedNotification.UnpublishedCultures"/> instead, because that operation is performed
    ///         as a publish.
    ///     </para>
    ///     <para>
    ///         Populated at raise-time (change tracking on the entity is reset once persisted, so it cannot be recovered
    ///         from <see cref="UnpublishedEntities"/> afterwards). <c>null</c> when no per-culture information was tracked.
    ///     </para>
    /// </remarks>
    public IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? UnpublishedCultures { get; }
}
