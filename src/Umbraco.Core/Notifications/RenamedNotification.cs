// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published after entities have been renamed.
/// </summary>
/// <typeparam name="T">The type of entity that was renamed.</typeparam>
/// <remarks>
///     This notification is published after entities have been successfully renamed,
///     allowing handlers to react for auditing or cache invalidation purposes.
/// </remarks>
public abstract class RenamedNotification<T> : EnumerableObjectNotification<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RenamedNotification{T}"/> class
    ///     with a single entity.
    /// </summary>
    /// <param name="target">The entity that was renamed.</param>
    /// <param name="messages">The event messages collection.</param>
    protected RenamedNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RenamedNotification{T}"/> class
    ///     with multiple entities.
    /// </summary>
    /// <param name="target">The entities that were renamed.</param>
    /// <param name="messages">The event messages collection.</param>
    protected RenamedNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the entities that were renamed.
    /// </summary>
    public IEnumerable<T> Entities => Target;
}
