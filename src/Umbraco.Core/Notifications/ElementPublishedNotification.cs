using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IElementService when the Publish method is called in the API and after data has been published.
/// Called after an element has been published.
/// </summary>
public sealed class ElementPublishedNotification : EnumerableObjectNotification<IElement>
{
    public ElementPublishedNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ElementPublishedNotification(IEnumerable<IElement> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementPublishedNotification"/> class with a single element
    ///     and the cultures that were published and unpublished.
    /// </summary>
    /// <param name="target">The element that was published.</param>
    /// <param name="messages">The event messages collection.</param>
    /// <param name="publishedCultures">The cultures that were published, keyed by element <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.</param>
    /// <param name="unpublishedCultures">The cultures that were unpublished as part of the publish operation, keyed by element <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.</param>
    public ElementPublishedNotification(
        IElement target,
        EventMessages messages,
        IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? publishedCultures,
        IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? unpublishedCultures)
        : base(target, messages)
    {
        PublishedCultures = publishedCultures;
        UnpublishedCultures = unpublishedCultures;
    }

    /// <summary>
    /// Gets a enumeration of <see cref="IElement"/> which are being published.
    /// </summary>
    public IEnumerable<IElement> PublishedEntities => Target;

    /// <summary>
    ///     Gets the cultures that were published for each element, keyed by element
    ///     <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.
    /// </summary>
    /// <remarks>
    ///     This reports every culture explicitly published in the operation (not only those whose data changed) — contrast
    ///     with <see cref="ElementSavedNotification.SavedCultures"/>, which is a changed-only delta. Populated at
    ///     raise-time (change tracking on the entity is reset once persisted, so it cannot be recovered from
    ///     <see cref="PublishedEntities"/> afterwards). For invariant elements the value is <c>["*"]</c>. The dictionary
    ///     itself is <c>null</c> only when the notification was raised without culture information (for example via a
    ///     constructor overload that does not accept it).
    /// </remarks>
    public IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? PublishedCultures { get; }

    /// <summary>
    ///     Gets the cultures that were unpublished as part of this publish operation, keyed by element
    ///     <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.
    /// </summary>
    /// <remarks>
    ///     Unpublishing an individual culture of a variant element is performed as a publish operation (the element is
    ///     re-published with that culture cleared), so the affected culture is reported <em>here</em> — not on an
    ///     <see cref="ElementUnpublishedNotification"/>, which is only raised when the element as a whole is unpublished.
    ///     Populated at raise-time; <c>null</c> when unpublishing a culture was not part of this operation at all (for
    ///     invariant elements, or a publish that took nothing down).
    /// </remarks>
    public IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? UnpublishedCultures { get; }
}
