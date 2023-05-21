using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class MemberGroupSavedNotification : SavedNotification<IMemberGroup>
{
    public MemberGroupSavedNotification(IMemberGroup target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MemberGroupSavedNotification(IEnumerable<IMemberGroup> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
