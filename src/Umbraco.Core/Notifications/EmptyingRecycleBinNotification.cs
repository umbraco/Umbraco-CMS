// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published before the recycle bin is emptied.
/// </summary>
/// <typeparam name="T">The type of entities in the recycle bin being deleted.</typeparam>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the empty recycle bin operation.
///     The notification is published before the entities are permanently deleted.
/// </remarks>
public abstract class EmptyingRecycleBinNotification<T> : StatefulNotification, ICancelableNotification
    where T : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EmptyingRecycleBinNotification{T}"/> class.
    /// </summary>
    /// <param name="deletedEntities">The collection of entities being permanently deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    protected EmptyingRecycleBinNotification(IEnumerable<T>? deletedEntities, EventMessages messages)
    {
        DeletedEntities = deletedEntities;
        Messages = messages;
    }

    /// <summary>
    ///     Gets the collection of entities being permanently deleted from the recycle bin.
    /// </summary>
    public IEnumerable<T>? DeletedEntities { get; }

    /// <summary>
    ///     Gets the event messages collection associated with this notification.
    /// </summary>
    public EventMessages Messages { get; }

    /// <inheritdoc />
    public bool Cancel { get; set; }
}
