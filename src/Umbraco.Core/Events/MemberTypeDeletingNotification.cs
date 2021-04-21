using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class MemberTypeDeletingNotification : DeletingNotification<IMemberType>
    {
        public MemberTypeDeletingNotification(IMemberType target, EventMessages messages) : base(target, messages)
        {
        }

        public MemberTypeDeletingNotification(IEnumerable<IMemberType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
