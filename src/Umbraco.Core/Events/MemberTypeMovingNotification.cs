using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class MemberTypeMovingNotification : MovingNotification<IMemberType>
    {
        public MemberTypeMovingNotification(MoveEventInfo<IMemberType> target, EventMessages messages) : base(target, messages)
        {
        }

        public MemberTypeMovingNotification(IEnumerable<MoveEventInfo<IMemberType>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
