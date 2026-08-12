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
    ///     Initializes a new instance of the <see cref="ContentPublishedNotification"/> class with multiple content items,
    ///     the descendant flag and the cultures that were published and unpublished per item.
    /// </summary>
    /// <param name="target">The collection of content items that were published.</param>
    /// <param name="messages">The event messages collection.</param>
    /// <param name="includeDescendants">A value indicating whether descendants were included in the publish operation.</param>
    /// <param name="publishedCultures">The cultures that were published, keyed by content <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.</param>
    /// <param name="unpublishedCultures">The cultures that were unpublished as part of the publish operation, keyed by content <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.</param>
    public ContentPublishedNotification(
        IEnumerable<IContent> target,
        EventMessages messages,
        bool includeDescendants,
        IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? publishedCultures,
        IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? unpublishedCultures)
        : base(target, messages)
    {
        IncludeDescendants = includeDescendants;
        PublishedCultures = publishedCultures;
        UnpublishedCultures = unpublishedCultures;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPublishedNotification"/> class with a single content item
    ///     and the cultures that were published and unpublished.
    /// </summary>
    /// <param name="target">The content item that was published.</param>
    /// <param name="messages">The event messages collection.</param>
    /// <param name="publishedCultures">The cultures that were published, keyed by content <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.</param>
    /// <param name="unpublishedCultures">The cultures that were unpublished as part of the publish operation, keyed by content <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.</param>
    public ContentPublishedNotification(
        IContent target,
        EventMessages messages,
        IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? publishedCultures,
        IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? unpublishedCultures)
        : base(target, messages)
    {
        PublishedCultures = publishedCultures;
        UnpublishedCultures = unpublishedCultures;
    }

    /// <summary>
    ///     Gets the collection of <see cref="IContent"/> items that were published.
    /// </summary>
    public IEnumerable<IContent> PublishedEntities => Target;

    /// <summary>
    ///     Gets a value indicating whether descendants were included in the publish operation.
    /// </summary>
    public bool IncludeDescendants { get; }

    /// <summary>
    ///     Gets the cultures that were published for each content item, keyed by content
    ///     <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.
    /// </summary>
    /// <remarks>
    ///     This reports every culture explicitly published in the operation (not only those whose data changed) — contrast
    ///     with <see cref="ContentSavedNotification.SavedCultures"/>, which is a changed-only delta. For a branch publish
    ///     (<see cref="IncludeDescendants"/> is <c>true</c>) each published document has its own entry. Populated at
    ///     raise-time (change tracking on the entity is reset once persisted, so it cannot be recovered from
    ///     <see cref="PublishedEntities"/> afterwards). For invariant content the value is <c>["*"]</c>. A document is
    ///     absent when no per-culture delta was tracked for it — e.g. descendants re-published as a side effect of
    ///     publishing an ancestor. The dictionary itself is <c>null</c> only when the notification was raised without
    ///     culture information (for example via a constructor overload that does not accept it).
    /// </remarks>
    public IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? PublishedCultures { get; }

    /// <summary>
    ///     Gets the cultures that were unpublished as part of this publish operation, keyed by content
    ///     <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Unpublishing an individual culture of a variant document is performed as a publish operation (the document is
    ///         re-published with that culture cleared), so the affected culture is reported <em>here</em> — not on a
    ///         <see cref="ContentUnpublishedNotification"/>, which is only raised when the document as a whole is unpublished.
    ///     </para>
    ///     <para>
    ///         Note that unpublishing several cultures at once is processed one culture at a time, so it raises one
    ///         <see cref="ContentPublishedNotification"/> per culture — each carrying a single entry here — rather than one
    ///         notification listing them all. Populated at raise-time; <c>null</c> when unpublishing a culture was not part
    ///         of this operation at all (for invariant content, or a publish that took nothing down).
    ///     </para>
    /// </remarks>
    public IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? UnpublishedCultures { get; }
}
