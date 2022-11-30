// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public abstract class MovedNotification<T> : ObjectNotification<IEnumerable<MoveEventInfo<T>>>
{
    protected MovedNotification(MoveEventInfo<T> target, EventMessages messages)
        : base(new[] { target }, messages)
    {
    }

    protected MovedNotification(IEnumerable<MoveEventInfo<T>> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    /// Gets a enumeration of <see cref="MoveEventInfo{TEntity}"/> with the moved entities.
    /// </summary>
    public IEnumerable<MoveEventInfo<T>> MoveInfoCollection => Target;
}
