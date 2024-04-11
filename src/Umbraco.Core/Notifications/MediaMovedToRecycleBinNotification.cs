// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMediaService when the MoveToRecycleBin method is called in the API, after the media object has been moved to the RecycleBin.
/// </summary>
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
