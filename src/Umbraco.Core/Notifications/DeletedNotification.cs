// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published after entities have been deleted.
/// </summary>
/// <typeparam name="T">The type of entities that were deleted.</typeparam>
/// <remarks>
///     <para>
///         This notification is published after the entities have been removed from the database.
///         It is not cancelable since the delete operation has already completed.
///     </para>
///     <para>
///         Its <see cref="StatefulNotification.State"/> is that of the <see cref="DeletingNotification{T}"/> that started the
///         operation, and is shared with the other deleted notifications from the same operation. That notification may not
///         list the entities in this one, for example when they are descendants of the entity chosen for deletion.
///     </para>
/// </remarks>
public abstract class DeletedNotification<T> : EnumerableObjectNotification<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DeletedNotification{T}"/> class with a single entity.
    /// </summary>
    /// <param name="target">The entity that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    protected DeletedNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeletedNotification{T}"/> class with multiple entities.
    /// </summary>
    /// <param name="target">The collection of entities that were deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    protected DeletedNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the collection of entities that were deleted.
    /// </summary>
    public IEnumerable<T> DeletedEntities => Target;
}
