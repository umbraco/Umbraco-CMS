// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published after an entity has been rolled back to a previous version.
/// </summary>
/// <typeparam name="T">The type of entity that was rolled back.</typeparam>
/// <remarks>
///     This notification is published after the entity has been reverted to its previous state.
///     It is not cancelable since the rollback operation has already completed.
/// </remarks>
public abstract class RolledBackNotification<T> : ObjectNotification<T>
    where T : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RolledBackNotification{T}"/> class.
    /// </summary>
    /// <param name="target">The entity that was rolled back.</param>
    /// <param name="messages">The event messages collection.</param>
    protected RolledBackNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the entity that was rolled back.
    /// </summary>
    public T Entity => Target;
}
