using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class DictionaryItemMovedNotification : MovedNotification<IDictionaryItem>
{
    public DictionaryItemMovedNotification(MoveEventInfo<IDictionaryItem> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public DictionaryItemMovedNotification(IEnumerable<MoveEventInfo<IDictionaryItem>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
