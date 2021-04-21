using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class MemberTypeSavedNotification : SavedNotification<IMemberType>
    {
        public MemberTypeSavedNotification(IMemberType target, EventMessages messages) : base(target, messages)
        {
        }

        public MemberTypeSavedNotification(IEnumerable<IMemberType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
