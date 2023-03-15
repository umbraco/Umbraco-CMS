// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class CopiedNotification<T> : ObjectNotification<T>
    where T : class
{
    protected CopiedNotification(T original, T copy, int parentId, Guid? parentKey, bool relateToOriginal, EventMessages messages)
        : base(original, messages)
    {
        Copy = copy;
        ParentId = parentId;
        ParentKey = parentKey;
        RelateToOriginal = relateToOriginal;
    }

    [Obsolete("Please use constructor that takes a parent key, scheduled for removal in V15")]
    protected CopiedNotification(T original, T copy, int parentId, bool relateToOriginal, EventMessages messages)
        : this(original, copy, parentId, null, relateToOriginal, messages)
    {
    }

    public T Original => Target;

    public T Copy { get; }

    [Obsolete("Please use parent key instead, scheduled for removal in V15")]
    public int ParentId { get; }

    public Guid? ParentKey { get; }

    public bool RelateToOriginal { get; }
}
