using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before elements are deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation. It is published when an
///     element is deleted, when elements of a type are deleted, and when the element recycle bin is emptied, in which case
///     it lists the elements at the root of the recycle bin. Elements inside folders are deleted along with the folder; see
///     <see cref="EntityContainerDeletingNotification"/>.
/// </remarks>
public sealed class ElementDeletingNotification : DeletingNotification<IElement>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementDeletingNotification"/> class with a single element.
    /// </summary>
    /// <param name="target">The element being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ElementDeletingNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementDeletingNotification"/> class with multiple elements.
    /// </summary>
    /// <param name="target">The elements being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ElementDeletingNotification(IEnumerable<IElement> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
