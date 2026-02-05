// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for cancelable notifications that carry a collection of target objects.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <remarks>
///     This class combines the functionality of <see cref="CancelableObjectNotification{T}"/> with
///     <see cref="IEnumerable{T}"/> as the target type, allowing handlers to both access the collection
///     of items and cancel the operation if needed.
/// </remarks>
public abstract class CancelableEnumerableObjectNotification<T> : CancelableObjectNotification<IEnumerable<T>>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CancelableEnumerableObjectNotification{T}"/> class with a single target item.
    /// </summary>
    /// <param name="target">The single target item.</param>
    /// <param name="messages">The event messages collection.</param>
    protected CancelableEnumerableObjectNotification(T target, EventMessages messages)
        : base(new[] { target }, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancelableEnumerableObjectNotification{T}"/> class with multiple target items.
    /// </summary>
    /// <param name="target">The collection of target items.</param>
    /// <param name="messages">The event messages collection.</param>
    protected CancelableEnumerableObjectNotification(IEnumerable<T> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }
}
