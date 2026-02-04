// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published before an entity is copied.
/// </summary>
/// <typeparam name="T">The type of entity being copied.</typeparam>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the copy operation.
///     The notification is published before the copy is persisted to the database.
/// </remarks>
public abstract class CopyingNotification<T> : CancelableObjectNotification<T>
    where T : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CopyingNotification{T}"/> class.
    /// </summary>
    /// <param name="original">The original entity being copied.</param>
    /// <param name="copy">The copy of the entity.</param>
    /// <param name="parentId">The ID of the new parent.</param>
    /// <param name="parentKey">The key of the new parent.</param>
    /// <param name="messages">The event messages collection.</param>
    protected CopyingNotification(T original, T copy, int parentId, Guid? parentKey, EventMessages messages)
        : base(original, messages)
    {
        Copy = copy;
        ParentId = parentId;
        ParentKey = parentKey;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CopyingNotification{T}"/> class.
    /// </summary>
    /// <param name="original">The original entity being copied.</param>
    /// <param name="copy">The copy of the entity.</param>
    /// <param name="parentId">The ID of the new parent.</param>
    /// <param name="messages">The event messages collection.</param>
    [Obsolete("Please use constructor that takes a parent key, scheduled for removal in V15")]
    protected CopyingNotification(T original, T copy, int parentId, EventMessages messages)
        : this(original, copy, parentId, null, messages)
    {
    }

    /// <summary>
    ///     Gets the original entity being copied.
    /// </summary>
    public T Original => Target;

    /// <summary>
    ///     Gets the copy of the entity.
    /// </summary>
    public T Copy { get; }

    /// <summary>
    ///     Gets the ID of the new parent.
    /// </summary>
    [Obsolete("Please use parent key instead, scheduled for removal in V15")]
    public int ParentId { get; }

    /// <summary>
    ///     Gets the key of the new parent.
    /// </summary>
    public Guid? ParentKey { get; }
}
