// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for cancelable notifications published before entities are renamed.
/// </summary>
/// <typeparam name="T">The type of entity being renamed.</typeparam>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the rename operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public abstract class RenamingNotification<T> : CancelableEnumerableObjectNotification<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RenamingNotification{T}"/> class
    ///     with a single entity.
    /// </summary>
    /// <param name="target">The entity being renamed.</param>
    /// <param name="messages">The event messages collection.</param>
    protected RenamingNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RenamingNotification{T}"/> class
    ///     with multiple entities.
    /// </summary>
    /// <param name="target">The entities being renamed.</param>
    /// <param name="messages">The event messages collection.</param>
    protected RenamingNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the entities being renamed.
    /// </summary>
    public IEnumerable<T> Entities => Target;
}
