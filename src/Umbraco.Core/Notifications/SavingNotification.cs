// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published before entities are saved.
/// </summary>
/// <typeparam name="T">The type of entities being saved.</typeparam>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation.
///     The notification is published before the entities are persisted to the database.
/// </remarks>
public abstract class SavingNotification<T> : CancelableEnumerableObjectNotification<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SavingNotification{T}"/> class with a single entity.
    /// </summary>
    /// <param name="target">The entity being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    protected SavingNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SavingNotification{T}"/> class with multiple entities.
    /// </summary>
    /// <param name="target">The collection of entities being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    protected SavingNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the collection of entities being saved.
    /// </summary>
    public IEnumerable<T> SavedEntities => Target;
}
