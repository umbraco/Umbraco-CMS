using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class MemberTypeDeletingNotification : DeletingNotification<IMemberType>
{
    public MemberTypeDeletingNotification(IMemberType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MemberTypeDeletingNotification(IEnumerable<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
