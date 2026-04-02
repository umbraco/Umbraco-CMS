// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published after entities have been moved to the recycle bin.
/// </summary>
/// <typeparam name="T">The type of entities that were moved to the recycle bin.</typeparam>
/// <remarks>
///     This notification is published after the entities have been moved to the recycle bin.
///     It is not cancelable since the operation has already completed.
/// </remarks>
public abstract class MovedToRecycleBinNotification<T> : ObjectNotification<IEnumerable<MoveToRecycleBinEventInfo<T>>>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MovedToRecycleBinNotification{T}"/> class with a single move operation.
    /// </summary>
    /// <param name="target">The move to recycle bin information for the entity that was moved.</param>
    /// <param name="messages">The event messages collection.</param>
    protected MovedToRecycleBinNotification(MoveToRecycleBinEventInfo<T> target, EventMessages messages)
        : base(new[] { target }, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MovedToRecycleBinNotification{T}"/> class with multiple move operations.
    /// </summary>
    /// <param name="target">The collection of move to recycle bin information for entities that were moved.</param>
    /// <param name="messages">The event messages collection.</param>
    protected MovedToRecycleBinNotification(IEnumerable<MoveToRecycleBinEventInfo<T>> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets an enumeration of <see cref="MoveToRecycleBinEventInfo{T}"/> with the entities that were moved to the recycle bin.
    /// </summary>
    public IEnumerable<MoveToRecycleBinEventInfo<T>> MoveInfoCollection => Target;
}
