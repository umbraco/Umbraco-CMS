// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public sealed class MediaMovedNotification : MovedNotification<IMedia>
    {
        public MediaMovedNotification(MoveEventInfo<IMedia> target, EventMessages messages) : base(target, messages)
        {
        }

        public MediaMovedNotification(IEnumerable<MoveEventInfo<IMedia>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
