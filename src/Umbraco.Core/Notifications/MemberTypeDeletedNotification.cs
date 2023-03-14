using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class MemberTypeDeletedNotification : DeletedNotification<IMemberType>
{
    public MemberTypeDeletedNotification(IMemberType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MemberTypeDeletedNotification(IEnumerable<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
