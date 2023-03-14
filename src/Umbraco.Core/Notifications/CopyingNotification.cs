// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class CopyingNotification<T> : CancelableObjectNotification<T>
    where T : class
{
    protected CopyingNotification(T original, T copy, int parentId, EventMessages messages)
        : base(original, messages)
    {
        Copy = copy;
        ParentId = parentId;
    }

    public T Original => Target;

    public T Copy { get; }

    public int ParentId { get; }
}
