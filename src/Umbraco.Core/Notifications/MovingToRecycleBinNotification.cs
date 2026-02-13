// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published before entities are moved to the recycle bin.
/// </summary>
/// <typeparam name="T">The type of entities being moved to the recycle bin.</typeparam>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the move to recycle bin operation.
///     The notification is published before the entities are moved.
/// </remarks>
public abstract class MovingToRecycleBinNotification<T> : CancelableObjectNotification<IEnumerable<MoveToRecycleBinEventInfo<T>>>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MovingToRecycleBinNotification{T}"/> class with a single move operation.
    /// </summary>
    /// <param name="target">The move to recycle bin information for the entity being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    protected MovingToRecycleBinNotification(MoveToRecycleBinEventInfo<T> target, EventMessages messages)
        : base(new[] { target }, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MovingToRecycleBinNotification{T}"/> class with multiple move operations.
    /// </summary>
    /// <param name="target">The collection of move to recycle bin information for entities being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    protected MovingToRecycleBinNotification(IEnumerable<MoveToRecycleBinEventInfo<T>> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets an enumeration of <see cref="MoveToRecycleBinEventInfo{T}"/> with the entities being moved to the recycle bin.
    /// </summary>
    public IEnumerable<MoveToRecycleBinEventInfo<T>> MoveInfoCollection => Target;
}
