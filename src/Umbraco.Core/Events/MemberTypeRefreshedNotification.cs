using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Events
{
    public class MemberTypeRefreshedNotification : ContentTypeRefreshNotification<IMemberType>
    {
        public MemberTypeRefreshedNotification(ContentTypeChange<IMemberType> target, EventMessages messages) : base(target, messages)
        {
        }

        public MemberTypeRefreshedNotification(IEnumerable<ContentTypeChange<IMemberType>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
