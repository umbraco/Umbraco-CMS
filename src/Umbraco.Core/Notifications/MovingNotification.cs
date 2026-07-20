// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published before entities are moved.
/// </summary>
/// <typeparam name="T">The type of entities being moved.</typeparam>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the move operation.
///     The notification is published before the entities are moved to their new location.
/// </remarks>
public abstract class MovingNotification<T> : CancelableObjectNotification<IEnumerable<MoveEventInfo<T>>>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MovingNotification{T}"/> class with a single move operation.
    /// </summary>
    /// <param name="target">The move information for the entity being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    protected MovingNotification(MoveEventInfo<T> target, EventMessages messages)
        : base(new[] { target }, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MovingNotification{T}"/> class with multiple move operations.
    /// </summary>
    /// <param name="target">The collection of move information for entities being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    protected MovingNotification(IEnumerable<MoveEventInfo<T>> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets an enumeration of <see cref="MoveEventInfo{T}"/> with the moving entities.
    /// </summary>
    public IEnumerable<MoveEventInfo<T>> MoveInfoCollection => Target;
}
