// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public sealed class MovingToRecycleBinNotification<T> : CancelableObjectNotification<IEnumerable<MoveEventInfo<T>>>
    {
        public MovingToRecycleBinNotification(MoveEventInfo<T> target, EventMessages messages) : base(new[] { target }, messages)
        {
        }

        public MovingToRecycleBinNotification(IEnumerable<MoveEventInfo<T>> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<MoveEventInfo<T>> MoveInfoCollection => Target;
    }
}
