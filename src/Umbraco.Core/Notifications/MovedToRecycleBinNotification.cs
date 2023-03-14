// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class MovedToRecycleBinNotification<T> : ObjectNotification<IEnumerable<MoveEventInfo<T>>>
{
    protected MovedToRecycleBinNotification(MoveEventInfo<T> target, EventMessages messages)
        : base(new[] { target }, messages)
    {
    }

    protected MovedToRecycleBinNotification(IEnumerable<MoveEventInfo<T>> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public IEnumerable<MoveEventInfo<T>> MoveInfoCollection => Target;
}
