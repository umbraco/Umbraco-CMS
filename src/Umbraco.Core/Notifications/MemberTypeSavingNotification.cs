using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class MemberTypeSavingNotification : SavingNotification<IMemberType>
{
    public MemberTypeSavingNotification(IMemberType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MemberTypeSavingNotification(IEnumerable<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
