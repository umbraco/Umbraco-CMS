using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class MediaTypeMovingNotification : MovingNotification<IMediaType>
{
    public MediaTypeMovingNotification(MoveEventInfo<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MediaTypeMovingNotification(IEnumerable<MoveEventInfo<IMediaType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
