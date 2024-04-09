// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class MediaMovedToRecycleBinNotification : MovedToRecycleBinNotification<IMedia>
{
    public MediaMovedToRecycleBinNotification(MoveToRecycleBinEventInfo<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MediaMovedToRecycleBinNotification(IEnumerable<MoveToRecycleBinEventInfo<IMedia>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
