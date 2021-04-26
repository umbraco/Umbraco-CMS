// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class MediaMovingToRecycleBinNotification : MovingToRecycleBinNotification<IMedia>
    {
        public MediaMovingToRecycleBinNotification(MoveEventInfo<IMedia> target, EventMessages messages) : base(target, messages)
        {
        }

        public MediaMovingToRecycleBinNotification(IEnumerable<MoveEventInfo<IMedia>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
