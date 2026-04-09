// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published after entities have been moved.
/// </summary>
/// <typeparam name="T">The type of entities that were moved.</typeparam>
/// <remarks>
///     This notification is published after the entities have been moved to their new location.
///     It is not cancelable since the move operation has already completed.
/// </remarks>
public abstract class MovedNotification<T> : ObjectNotification<IEnumerable<MoveEventInfo<T>>>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MovedNotification{T}"/> class with a single move operation.
    /// </summary>
    /// <param name="target">The move information for the entity that was moved.</param>
    /// <param name="messages">The event messages collection.</param>
    protected MovedNotification(MoveEventInfo<T> target, EventMessages messages)
        : base(new[] { target }, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MovedNotification{T}"/> class with multiple move operations.
    /// </summary>
    /// <param name="target">The collection of move information for entities that were moved.</param>
    /// <param name="messages">The event messages collection.</param>
    protected MovedNotification(IEnumerable<MoveEventInfo<T>> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets an enumeration of <see cref="MoveEventInfo{T}"/> with the moved entities.
    /// </summary>
    public IEnumerable<MoveEventInfo<T>> MoveInfoCollection => Target;
}
