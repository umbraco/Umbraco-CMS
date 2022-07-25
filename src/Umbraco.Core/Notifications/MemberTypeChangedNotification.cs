using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

public class MemberTypeChangedNotification : ContentTypeChangeNotification<IMemberType>
{
    public MemberTypeChangedNotification(ContentTypeChange<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MemberTypeChangedNotification(IEnumerable<ContentTypeChange<IMemberType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
