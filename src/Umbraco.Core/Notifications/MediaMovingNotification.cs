// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class MediaMovingNotification : MovingNotification<IMedia>
{
    public MediaMovingNotification(MoveEventInfo<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MediaMovingNotification(IEnumerable<MoveEventInfo<IMedia>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
