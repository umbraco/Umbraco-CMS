// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class MediaMovedToRecycleBinNotification : MovedToRecycleBinNotification<IMedia>
    {
        public MediaMovedToRecycleBinNotification(MoveEventInfo<IMedia> target, EventMessages messages) : base(target, messages)
        {
        }

        public MediaMovedToRecycleBinNotification(IEnumerable<MoveEventInfo<IMedia>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
