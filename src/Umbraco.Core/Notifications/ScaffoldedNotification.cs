// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for cancelable notifications published when entities are scaffolded (copied).
/// </summary>
/// <typeparam name="T">The type of entity being scaffolded.</typeparam>
/// <remarks>
///     This notification provides both the original entity and the scaffold (copy) being created.
/// </remarks>
public abstract class ScaffoldedNotification<T> : CancelableObjectNotification<T>
    where T : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ScaffoldedNotification{T}"/> class.
    /// </summary>
    /// <param name="original">The original entity being copied from.</param>
    /// <param name="scaffold">The scaffold (copy) being created.</param>
    /// <param name="parentId">The ID of the parent under which the scaffold will be created.</param>
    /// <param name="messages">The event messages collection.</param>
    protected ScaffoldedNotification(T original, T scaffold, int parentId, EventMessages messages)
        : base(original, messages)
    {
        Scaffold = scaffold;
        ParentId = parentId;
    }

    /// <summary>
    ///     Gets the original entity being copied from.
    /// </summary>
    public T Original => Target;

    /// <summary>
    ///     Gets the scaffold (copy) being created.
    /// </summary>
    public T Scaffold { get; }

    /// <summary>
    ///     Gets the ID of the parent under which the scaffold will be created.
    /// </summary>
    public int ParentId { get; }
}
