using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class MemberTypeDeletedNotification : DeletedNotification<IMemberType>
    {
        public MemberTypeDeletedNotification(IMemberType target, EventMessages messages) : base(target, messages)
        {
        }

        public MemberTypeDeletedNotification(IEnumerable<IMemberType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
