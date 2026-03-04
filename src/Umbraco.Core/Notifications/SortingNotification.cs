// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published before entities are sorted.
/// </summary>
/// <typeparam name="T">The type of entities being sorted.</typeparam>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the sort operation.
///     The notification is published before the new sort order is persisted.
/// </remarks>
public abstract class SortingNotification<T> : CancelableEnumerableObjectNotification<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SortingNotification{T}"/> class.
    /// </summary>
    /// <param name="target">The collection of entities being sorted.</param>
    /// <param name="messages">The event messages collection.</param>
    protected SortingNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the collection of entities being sorted.
    /// </summary>
    public IEnumerable<T> SortedEntities => Target;
}
