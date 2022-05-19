// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class RolledBackNotification<T> : ObjectNotification<T>
    where T : class
{
    protected RolledBackNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    public T Entity => Target;
}
