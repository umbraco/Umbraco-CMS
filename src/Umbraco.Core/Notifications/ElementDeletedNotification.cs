using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after an element has been deleted.
/// </summary>
/// <remarks>
///     <para>
///         This notification is published when an element is deleted, when elements of a type are deleted, when an
///         element folder is deleted from the recycle bin, and when the element recycle bin is emptied.
///         It is not cancelable since the delete operation has already completed.
///     </para>
///     <para>
///         An element deleted along with a folder, whether the folder was deleted on its own or by emptying the recycle
///         bin, shares the state of that folder's <see cref="EntityContainerDeletingNotification"/>. Otherwise it shares the
///         state of the <see cref="ElementDeletingNotification"/> that started the operation.
///     </para>
/// </remarks>
public sealed class ElementDeletedNotification : DeletedNotification<IElement>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementDeletedNotification"/> class.
    /// </summary>
    /// <param name="target">The element that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ElementDeletedNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }
}
