// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class SortedNotification<T> : EnumerableObjectNotification<T>
{
    protected SortedNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public IEnumerable<T> SortedEntities => Target;
}
