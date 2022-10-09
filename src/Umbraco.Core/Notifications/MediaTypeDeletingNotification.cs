using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class MediaTypeDeletingNotification : DeletingNotification<IMediaType>
{
    public MediaTypeDeletingNotification(IMediaType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MediaTypeDeletingNotification(IEnumerable<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
