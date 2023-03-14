using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class MemberTypeSavedNotification : SavedNotification<IMemberType>
{
    public MemberTypeSavedNotification(IMemberType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MemberTypeSavedNotification(IEnumerable<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
