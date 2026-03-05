// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications related to tree structure changes.
/// </summary>
/// <typeparam name="T">The type of entity in the tree.</typeparam>
/// <remarks>
///     Tree change notifications are published when entities are added, removed, moved, or have their
///     structure modified in a hierarchical tree. They are used for cache invalidation and tree synchronization.
/// </remarks>
public abstract class TreeChangeNotification<T> : EnumerableObjectNotification<TreeChange<T>>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TreeChangeNotification{T}"/> class
    ///     with a single tree change.
    /// </summary>
    /// <param name="target">The tree change that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    protected TreeChangeNotification(TreeChange<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TreeChangeNotification{T}"/> class
    ///     with multiple tree changes.
    /// </summary>
    /// <param name="target">The collection of tree changes that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    protected TreeChangeNotification(IEnumerable<TreeChange<T>> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the tree changes that occurred.
    /// </summary>
    public IEnumerable<TreeChange<T>> Changes => Target;
}
