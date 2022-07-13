// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class CreatedNotification<T> : ObjectNotification<T>
    where T : class
{
    protected CreatedNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    public T CreatedEntity => Target;
}
