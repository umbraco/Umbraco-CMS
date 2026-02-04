// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published after an entity has been copied.
/// </summary>
/// <typeparam name="T">The type of entity that was copied.</typeparam>
/// <remarks>
///     This notification is published after the copy has been persisted to the database.
///     It is not cancelable since the copy operation has already completed.
/// </remarks>
public abstract class CopiedNotification<T> : ObjectNotification<T>
    where T : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CopiedNotification{T}"/> class.
    /// </summary>
    /// <param name="original">The original entity that was copied.</param>
    /// <param name="copy">The copy of the entity.</param>
    /// <param name="parentId">The ID of the new parent.</param>
    /// <param name="parentKey">The key of the new parent.</param>
    /// <param name="relateToOriginal">A value indicating whether the copy is related to the original.</param>
    /// <param name="messages">The event messages collection.</param>
    protected CopiedNotification(T original, T copy, int parentId, Guid? parentKey, bool relateToOriginal, EventMessages messages)
        : base(original, messages)
    {
        Copy = copy;
        ParentId = parentId;
        ParentKey = parentKey;
        RelateToOriginal = relateToOriginal;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CopiedNotification{T}"/> class.
    /// </summary>
    /// <param name="original">The original entity that was copied.</param>
    /// <param name="copy">The copy of the entity.</param>
    /// <param name="parentId">The ID of the new parent.</param>
    /// <param name="relateToOriginal">A value indicating whether the copy is related to the original.</param>
    /// <param name="messages">The event messages collection.</param>
    [Obsolete("Please use constructor that takes a parent key, scheduled for removal in V15")]
    protected CopiedNotification(T original, T copy, int parentId, bool relateToOriginal, EventMessages messages)
        : this(original, copy, parentId, null, relateToOriginal, messages)
    {
    }

    /// <summary>
    ///     Gets the original entity that was copied.
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

    /// <summary>
    ///     Gets a value indicating whether the copy is related to the original.
    /// </summary>
    public bool RelateToOriginal { get; }
}
