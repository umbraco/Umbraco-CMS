// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published after an entity has been created.
/// </summary>
/// <typeparam name="T">The type of entity that was created.</typeparam>
/// <remarks>
///     This notification is published after the entity has been persisted to the database.
///     It is not cancelable since the creation has already completed.
/// </remarks>
public abstract class CreatedNotification<T> : ObjectNotification<T>
    where T : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CreatedNotification{T}"/> class.
    /// </summary>
    /// <param name="target">The entity that was created.</param>
    /// <param name="messages">The event messages collection.</param>
    protected CreatedNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the entity that was created.
    /// </summary>
    public T CreatedEntity => Target;
}
