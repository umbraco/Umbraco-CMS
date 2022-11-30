using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class MediaTypeSavingNotification : SavingNotification<IMediaType>
{
    public MediaTypeSavingNotification(IMediaType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MediaTypeSavingNotification(IEnumerable<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
