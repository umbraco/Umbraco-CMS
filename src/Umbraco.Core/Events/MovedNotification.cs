// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;

namespace Umbraco.Cms.Core.Events
{
    public abstract class MovedNotification<T> : ObjectNotification<IEnumerable<MoveEventInfo<T>>>
    {
        protected MovedNotification(MoveEventInfo<T> target, EventMessages messages) : base(new[] { target }, messages)
        {
        }

        protected MovedNotification(IEnumerable<MoveEventInfo<T>> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<MoveEventInfo<T>> MoveInfoCollection => Target;
    }
}
