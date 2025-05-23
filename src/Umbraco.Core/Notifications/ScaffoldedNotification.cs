// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class ScaffoldedNotification<T> : CancelableObjectNotification<T>
    where T : class
{
    protected ScaffoldedNotification(T original, T scaffold, int parentId, EventMessages messages)
        : base(original, messages)
    {
        Scaffold = scaffold;
        ParentId = parentId;
    }

    public T Original => Target;

    public T Scaffold { get; }

    public int ParentId { get; }
}
