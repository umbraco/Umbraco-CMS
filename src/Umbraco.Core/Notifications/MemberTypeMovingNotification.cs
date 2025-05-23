using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMemberTypeService when the Move method is called in the API.
/// </summary>
public class MemberTypeMovingNotification : MovingNotification<IMemberType>
{
    public MemberTypeMovingNotification(MoveEventInfo<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MemberTypeMovingNotification(IEnumerable<MoveEventInfo<IMemberType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
