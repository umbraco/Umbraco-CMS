using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IElementService when the UnPublish method is called in the API and after data has been unpublished.
/// </summary>
public sealed class ElementUnpublishedNotification : EnumerableObjectNotification<IElement>
{
    public ElementUnpublishedNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ElementUnpublishedNotification(IEnumerable<IElement> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementUnpublishedNotification"/> class with a single element
    ///     and the cultures that were unpublished.
    /// </summary>
    /// <param name="target">The element that was unpublished.</param>
    /// <param name="messages">The event messages collection.</param>
    /// <param name="unpublishedCultures">The cultures that were unpublished, keyed by element <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.</param>
    public ElementUnpublishedNotification(IElement target, EventMessages messages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? unpublishedCultures)
        : base(target, messages)
        => UnpublishedCultures = unpublishedCultures;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementUnpublishedNotification"/> class with multiple elements
    ///     and the cultures that were unpublished.
    /// </summary>
    /// <param name="target">The collection of elements that were unpublished.</param>
    /// <param name="messages">The event messages collection.</param>
    /// <param name="unpublishedCultures">The cultures that were unpublished, keyed by element <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.</param>
    public ElementUnpublishedNotification(IEnumerable<IElement> target, EventMessages messages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? unpublishedCultures)
        : base(target, messages)
        => UnpublishedCultures = unpublishedCultures;

    /// <summary>
    /// Gets a enumeration of <see cref="IElement"/> which are being unpublished.
    /// </summary>
    public IEnumerable<IElement> UnpublishedEntities => Target;

    /// <summary>
    ///     Gets the cultures that were unpublished for each element, keyed by element
    ///     <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.
    /// </summary>
    /// <remarks>
    ///     This notification is only raised when an element is unpublished <em>as a whole</em>; the value therefore
    ///     reports every culture that was taken down (for invariant elements, <c>["*"]</c>). Unpublishing an individual
    ///     culture of a still-published variant element does not raise this notification — it is reported on
    ///     <see cref="ElementPublishedNotification.UnpublishedCultures"/> instead, because that operation is performed as a
    ///     publish. Populated at raise-time (change tracking on the entity is reset once persisted, so it cannot be
    ///     recovered from <see cref="UnpublishedEntities"/> afterwards). The dictionary is <c>null</c> only when the
    ///     notification was raised without culture information (for example via a constructor overload that does not
    ///     accept it).
    /// </remarks>
    public IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? UnpublishedCultures { get; }
}
