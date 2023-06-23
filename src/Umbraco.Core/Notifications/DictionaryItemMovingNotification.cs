using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class DictionaryItemMovingNotification : MovingNotification<IDictionaryItem>
{
    public DictionaryItemMovingNotification(MoveEventInfo<IDictionaryItem> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public DictionaryItemMovingNotification(IEnumerable<MoveEventInfo<IDictionaryItem>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
