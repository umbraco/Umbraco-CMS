// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMediaService when the MoveToRecycleBin method is called in the API.
/// </summary>
public sealed class MediaMovingToRecycleBinNotification : MovingToRecycleBinNotification<IMedia>
{
    public MediaMovingToRecycleBinNotification(MoveToRecycleBinEventInfo<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MediaMovingToRecycleBinNotification(IEnumerable<MoveToRecycleBinEventInfo<IMedia>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
