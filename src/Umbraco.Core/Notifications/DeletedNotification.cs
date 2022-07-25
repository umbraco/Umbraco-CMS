// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class DeletedNotification<T> : EnumerableObjectNotification<T>
{
    protected DeletedNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    protected DeletedNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public IEnumerable<T> DeletedEntities => Target;
}
