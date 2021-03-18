// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;

namespace Umbraco.Cms.Core.Events
{
    public abstract class MovingNotification<T> : CancelableObjectNotification<IEnumerable<MoveEventInfo<T>>>
    {
        protected MovingNotification(MoveEventInfo<T> target, EventMessages messages) : base(new[] {target}, messages)
        {
        }

        protected MovingNotification(IEnumerable<MoveEventInfo<T>> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<MoveEventInfo<T>> MoveInfoCollection => Target;
    }
}
