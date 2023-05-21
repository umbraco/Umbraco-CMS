// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class SavedNotification<T> : EnumerableObjectNotification<T>
{
    protected SavedNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    protected SavedNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public IEnumerable<T> SavedEntities => Target;
}
