// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class CancelableEnumerableObjectNotification<T> : CancelableObjectNotification<IEnumerable<T>>
{
    protected CancelableEnumerableObjectNotification(T target, EventMessages messages)
        : base(new[] { target }, messages)
    {
    }

    protected CancelableEnumerableObjectNotification(IEnumerable<T> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }
}
