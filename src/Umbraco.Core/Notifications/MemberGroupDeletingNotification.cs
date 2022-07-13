using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class MemberGroupDeletingNotification : DeletingNotification<IMemberGroup>
{
    public MemberGroupDeletingNotification(IMemberGroup target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MemberGroupDeletingNotification(IEnumerable<IMemberGroup> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
