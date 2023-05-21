using System.ComponentModel;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

[Obsolete("This is only used for the internal cache and will change, use tree change notifications instead")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class MemberTypeRefreshedNotification : ContentTypeRefreshNotification<IMemberType>
{
    public MemberTypeRefreshedNotification(ContentTypeChange<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MemberTypeRefreshedNotification(IEnumerable<ContentTypeChange<IMemberType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
