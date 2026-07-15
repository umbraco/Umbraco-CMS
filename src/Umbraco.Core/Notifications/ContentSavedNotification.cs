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

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSavedNotification"/> class with a single content item
    ///     and the cultures that were saved.
    /// </summary>
    /// <param name="target">The content item that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    /// <param name="savedCultures">The cultures that were saved, keyed by content <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.</param>
    public ContentSavedNotification(IContent target, EventMessages messages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? savedCultures)
        : base(target, messages)
        => SavedCultures = savedCultures;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSavedNotification"/> class with multiple content items
    ///     and the cultures that were saved.
    /// </summary>
    /// <param name="target">The collection of content items that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    /// <param name="savedCultures">The cultures that were saved, keyed by content <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.</param>
    public ContentSavedNotification(IEnumerable<IContent> target, EventMessages messages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? savedCultures)
        : base(target, messages)
        => SavedCultures = savedCultures;

    /// <summary>
    ///     Gets the cultures that were saved for each content item, keyed by content
    ///     <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This reports the cultures that actually <em>changed</em> in this save (the dirty delta), not every culture
    ///         on the document. A save that changes nothing for a given document therefore reports no cultures for it.
    ///         Contrast with <see cref="ContentPublishedNotification.PublishedCultures"/>, which reports every culture
    ///         explicitly published regardless of whether its data changed.
    ///     </para>
    ///     <para>
    ///         Populated at raise-time: change tracking on the entity is reset once persisted, so this cannot be recovered
    ///         from <see cref="Umbraco.Cms.Core.Notifications.SavedNotification{T}.SavedEntities"/> afterwards. For
    ///         invariant content the value is <c>["*"]</c>. A given document is absent from the dictionary when no culture
    ///         change was tracked for it — e.g. a no-op re-save of variant content, or a sort operation. The dictionary
    ///         itself is <c>null</c> only when the notification was raised without culture information (for example via a
    ///         constructor overload that does not accept it); a save that tracked cultures but changed none reports an
    ///         empty dictionary, not <c>null</c>.
    ///     </para>
    /// </remarks>
    public IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? SavedCultures { get; }
}
