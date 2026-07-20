// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published before an entity is created.
/// </summary>
/// <typeparam name="T">The type of entity being created.</typeparam>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the creation.
///     The notification is published before the entity is persisted to the database.
/// </remarks>
public abstract class CreatingNotification<T> : CancelableObjectNotification<T>
    where T : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CreatingNotification{T}"/> class.
    /// </summary>
    /// <param name="target">The entity being created.</param>
    /// <param name="messages">The event messages collection.</param>
    protected CreatingNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the entity being created.
    /// </summary>
    public T CreatedEntity => Target;
}
