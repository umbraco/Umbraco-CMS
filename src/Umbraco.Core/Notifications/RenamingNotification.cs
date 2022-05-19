// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class RenamingNotification<T> : CancelableEnumerableObjectNotification<T>
{
    protected RenamingNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    protected RenamingNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public IEnumerable<T> Entities => Target;
}
