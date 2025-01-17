// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class MovingToRecycleBinNotification<T> : CancelableObjectNotification<IEnumerable<MoveToRecycleBinEventInfo<T>>>
{
    protected MovingToRecycleBinNotification(MoveToRecycleBinEventInfo<T> target, EventMessages messages)
        : base(new[] { target }, messages)
    {
    }

    protected MovingToRecycleBinNotification(IEnumerable<MoveToRecycleBinEventInfo<T>> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public IEnumerable<MoveToRecycleBinEventInfo<T>> MoveInfoCollection => Target;
}
