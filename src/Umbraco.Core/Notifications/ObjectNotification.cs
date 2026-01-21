// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications that carry a target object.
/// </summary>
/// <typeparam name="T">The type of the target object associated with this notification.</typeparam>
/// <remarks>
///     This class extends <see cref="StatefulNotification"/> and provides a strongly-typed target object
///     that represents the entity being operated on.
/// </remarks>
public abstract class ObjectNotification<T> : StatefulNotification
    where T : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ObjectNotification{T}"/> class.
    /// </summary>
    /// <param name="target">The target object associated with this notification.</param>
    /// <param name="messages">The event messages collection.</param>
    protected ObjectNotification(T target, EventMessages messages)
    {
        Messages = messages;
        Target = target;
    }

    /// <summary>
    ///     Gets the event messages collection associated with this notification.
    /// </summary>
    public EventMessages Messages { get; }

    /// <summary>
    ///     Gets the target object associated with this notification.
    /// </summary>
    protected T Target { get; }
}
