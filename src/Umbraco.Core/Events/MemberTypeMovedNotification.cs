using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class MemberTypeMovedNotification : MovedNotification<IMemberType>
    {
        public MemberTypeMovedNotification(MoveEventInfo<IMemberType> target, EventMessages messages) : base(target, messages)
        {
        }

        public MemberTypeMovedNotification(IEnumerable<MoveEventInfo<IMemberType>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
