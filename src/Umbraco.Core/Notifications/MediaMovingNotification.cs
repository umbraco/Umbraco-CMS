// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMediaService when the Move method is called in the API.
///  NOTE: If the target parent is the Recycle bin, this notification is never published. Try the <see cref="MediaMovingToRecycleBinNotification"/> instead.
/// </summary>
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
