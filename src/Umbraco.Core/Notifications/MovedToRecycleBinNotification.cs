// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class MovedToRecycleBinNotification<T> : ObjectNotification<IEnumerable<MoveToRecycleBinEventInfo<T>>>
{
    protected MovedToRecycleBinNotification(MoveToRecycleBinEventInfo<T> target, EventMessages messages)
        : base(new[] { target }, messages)
    {
    }

    protected MovedToRecycleBinNotification(IEnumerable<MoveToRecycleBinEventInfo<T>> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public IEnumerable<MoveToRecycleBinEventInfo<T>> MoveInfoCollection => Target;
}
