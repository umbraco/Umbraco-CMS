using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class MediaTypeDeletedNotification : DeletedNotification<IMediaType>
{
    public MediaTypeDeletedNotification(IMediaType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MediaTypeDeletedNotification(IEnumerable<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
