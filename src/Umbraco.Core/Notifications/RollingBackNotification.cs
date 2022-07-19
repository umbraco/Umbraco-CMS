// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class RollingBackNotification<T> : CancelableObjectNotification<T>
    where T : class
{
    protected RollingBackNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    public T Entity => Target;
}
