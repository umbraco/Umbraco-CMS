// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public sealed class ContentMovingToRecycleBinNotification : MovingToRecycleBinNotification<IContent>
{
    public ContentMovingToRecycleBinNotification(MoveEventInfo<IContent> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }

    public ContentMovingToRecycleBinNotification(IEnumerable<MoveEventInfo<IContent>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
