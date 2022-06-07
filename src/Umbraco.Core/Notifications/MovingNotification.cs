// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class MovingNotification<T> : CancelableObjectNotification<IEnumerable<MoveEventInfo<T>>>
{
    protected MovingNotification(MoveEventInfo<T> target, EventMessages messages)
        : base(new[] { target }, messages)
    {
    }

    protected MovingNotification(IEnumerable<MoveEventInfo<T>> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public IEnumerable<MoveEventInfo<T>> MoveInfoCollection => Target;
}
