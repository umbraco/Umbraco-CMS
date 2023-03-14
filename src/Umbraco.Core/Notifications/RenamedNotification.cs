// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class RenamedNotification<T> : EnumerableObjectNotification<T>
{
    protected RenamedNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    protected RenamedNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public IEnumerable<T> Entities => Target;
}
