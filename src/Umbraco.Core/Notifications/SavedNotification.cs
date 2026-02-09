// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published after entities have been saved.
/// </summary>
/// <typeparam name="T">The type of entities that were saved.</typeparam>
/// <remarks>
///     This notification is published after the entities have been persisted to the database.
///     It is not cancelable since the save operation has already completed.
/// </remarks>
public abstract class SavedNotification<T> : EnumerableObjectNotification<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SavedNotification{T}"/> class with a single entity.
    /// </summary>
    /// <param name="target">The entity that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    protected SavedNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SavedNotification{T}"/> class with multiple entities.
    /// </summary>
    /// <param name="target">The collection of entities that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    protected SavedNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the collection of entities that were saved.
    /// </summary>
    public IEnumerable<T> SavedEntities => Target;
}
