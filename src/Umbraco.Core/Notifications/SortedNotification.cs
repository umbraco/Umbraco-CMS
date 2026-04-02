// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published after entities have been sorted.
/// </summary>
/// <typeparam name="T">The type of entities that were sorted.</typeparam>
/// <remarks>
///     This notification is published after the new sort order has been persisted.
///     It is not cancelable since the sort operation has already completed.
/// </remarks>
public abstract class SortedNotification<T> : EnumerableObjectNotification<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SortedNotification{T}"/> class.
    /// </summary>
    /// <param name="target">The collection of entities that were sorted.</param>
    /// <param name="messages">The event messages collection.</param>
    protected SortedNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the collection of entities that were sorted.
    /// </summary>
    public IEnumerable<T> SortedEntities => Target;
}
