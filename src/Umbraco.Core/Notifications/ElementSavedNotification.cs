using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IElementService when the Save method is called in the API and after the data has been persisted.
/// </summary>
public sealed class ElementSavedNotification : SavedNotification<IElement>
{
    /// <summary>
    ///  Initializes a new instance of the  <see cref="ElementSavedNotification"/>
    /// </summary>
    public ElementSavedNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    /// Gets a enumeration of <see cref="IElement"/>.
    /// </summary>
    public ElementSavedNotification(IEnumerable<IElement> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementSavedNotification"/> class with a single element
    ///     and the cultures that were saved.
    /// </summary>
    /// <param name="target">The element that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    /// <param name="savedCultures">The cultures that were saved, keyed by element <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.</param>
    public ElementSavedNotification(IElement target, EventMessages messages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? savedCultures)
        : base(target, messages)
        => SavedCultures = savedCultures;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementSavedNotification"/> class with multiple elements
    ///     and the cultures that were saved.
    /// </summary>
    /// <param name="target">The collection of elements that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    /// <param name="savedCultures">The cultures that were saved, keyed by element <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.</param>
    public ElementSavedNotification(IEnumerable<IElement> target, EventMessages messages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? savedCultures)
        : base(target, messages)
        => SavedCultures = savedCultures;

    /// <summary>
    ///     Gets the cultures that were saved for each element, keyed by element
    ///     <see cref="Umbraco.Cms.Core.Models.Entities.IEntity.Key"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This reports the cultures that actually <em>changed</em> in this save (the dirty delta), not every culture
    ///         on the element. A save that changes nothing for a given element therefore reports no cultures for it.
    ///         Contrast with <see cref="ElementPublishedNotification.PublishedCultures"/>, which reports every culture
    ///         explicitly published regardless of whether its data changed.
    ///     </para>
    ///     <para>
    ///         For variant elements the value is the set of cultures whose data changed; for invariant elements it is the
    ///         <c>["*"]</c> marker, reported only when the element actually changed. A given element is absent from the
    ///         dictionary when no change was tracked for it — e.g. a no-op re-save.
    ///     </para>
    ///     <para>
    ///         Populated at raise-time: change tracking on the entity is reset once persisted, so this cannot be recovered
    ///         from <see cref="Umbraco.Cms.Core.Notifications.SavedNotification{T}.SavedEntities"/> afterwards. The
    ///         dictionary itself is <c>null</c> only when the notification was raised without culture information (for
    ///         example via a constructor overload that does not accept it); a save that tracked cultures but changed none
    ///         reports an empty dictionary, not <c>null</c>.
    ///     </para>
    /// </remarks>
    public IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? SavedCultures { get; }
}
