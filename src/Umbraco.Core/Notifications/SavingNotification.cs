// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class SavingNotification<T> : CancelableEnumerableObjectNotification<T>
{
    protected SavingNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    protected SavingNotification(IEnumerable<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public IEnumerable<T> SavedEntities => Target;
}
