using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class MemberTypeSavingNotification : SavingNotification<IMemberType>
    {
        public MemberTypeSavingNotification(IMemberType target, EventMessages messages) : base(target, messages)
        {
        }

        public MemberTypeSavingNotification(IEnumerable<IMemberType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
