// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published before an entity is rolled back to a previous version.
/// </summary>
/// <typeparam name="T">The type of entity being rolled back.</typeparam>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the rollback operation.
///     The notification is published before the entity is reverted to its previous state.
/// </remarks>
public abstract class RollingBackNotification<T> : CancelableObjectNotification<T>
    where T : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RollingBackNotification{T}"/> class.
    /// </summary>
    /// <param name="target">The entity being rolled back.</param>
    /// <param name="messages">The event messages collection.</param>
    protected RollingBackNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the entity being rolled back.
    /// </summary>
    public T Entity => Target;
}
