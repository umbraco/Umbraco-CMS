// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published before entities are deleted.
/// </summary>
/// <typeparam name="T">The type of entities being deleted.</typeparam>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation.
///     The notification is published before the entities are removed from the database.
/// </remarks>
public abstract class DeletingNotification<T> : CancelableEnumerableObjectNotification<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DeletingNotification{T}"/> class with a single entity.
    /// </summary>
    /// <param name="target">The entity being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    protected DeletingNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeletingNotification{T}"/> class with multiple entities.
    /// </summary>
    /// <param name="target">The collection of entities being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    protected DeletingNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the collection of entities being deleted.
    /// </summary>
    public IEnumerable<T> DeletedEntities => Target;
}
