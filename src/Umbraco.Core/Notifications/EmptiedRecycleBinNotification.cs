// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published after the recycle bin has been emptied.
/// </summary>
/// <typeparam name="T">The type of entities that were in the recycle bin.</typeparam>
/// <remarks>
///     This notification is published after the entities have been permanently deleted.
///     It is not cancelable since the empty operation has already completed.
/// </remarks>
public abstract class EmptiedRecycleBinNotification<T> : StatefulNotification
    where T : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EmptiedRecycleBinNotification{T}"/> class.
    /// </summary>
    /// <param name="deletedEntities">The collection of entities that were permanently deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    protected EmptiedRecycleBinNotification(IEnumerable<T> deletedEntities, EventMessages messages)
    {
        DeletedEntities = deletedEntities;
        Messages = messages;
    }

    /// <summary>
    ///     Gets the collection of entities that were permanently deleted from the recycle bin.
    /// </summary>
    public IEnumerable<T> DeletedEntities { get; }

    /// <summary>
    ///     Gets the event messages collection associated with this notification.
    /// </summary>
    public EventMessages Messages { get; }
}
