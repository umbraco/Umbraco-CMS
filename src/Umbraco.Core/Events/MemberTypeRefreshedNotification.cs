using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Events
{
    public class MemberTypeRefreshNotification : ContentTypeRefreshNotification<IMemberType>
    {
        public MemberTypeRefreshNotification(ContentTypeChange<IMemberType> target, EventMessages messages) : base(target, messages)
        {
        }

        public MemberTypeRefreshNotification(IEnumerable<ContentTypeChange<IMemberType>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
