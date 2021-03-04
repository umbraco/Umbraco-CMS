// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public sealed class MovedNotification<T> : ObjectNotification<IEnumerable<MoveEventInfo<T>>>
    {
        public MovedNotification(MoveEventInfo<T> target, EventMessages messages) : base(new[] { target }, messages)
        {
        }

        public MovedNotification(IEnumerable<MoveEventInfo<T>> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<MoveEventInfo<T>> MoveInfoCollection => Target;
    }
}
