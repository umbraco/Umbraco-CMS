// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications that carry a collection of target objects.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <remarks>
///     This class extends <see cref="ObjectNotification{T}"/> with <see cref="IEnumerable{T}"/> as the target type,
///     providing convenient constructors for both single items and collections.
/// </remarks>
public abstract class EnumerableObjectNotification<T> : ObjectNotification<IEnumerable<T>>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EnumerableObjectNotification{T}"/> class with a single target item.
    /// </summary>
    /// <param name="target">The single target item.</param>
    /// <param name="messages">The event messages collection.</param>
    protected EnumerableObjectNotification(T target, EventMessages messages)
        : base(new[] { target }, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EnumerableObjectNotification{T}"/> class with multiple target items.
    /// </summary>
    /// <param name="target">The collection of target items.</param>
    /// <param name="messages">The event messages collection.</param>
    protected EnumerableObjectNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
