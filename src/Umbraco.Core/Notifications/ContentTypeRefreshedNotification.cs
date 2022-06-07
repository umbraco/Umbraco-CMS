using System.ComponentModel;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

[Obsolete("This is only used for the internal cache and will change, use saved notifications instead")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class ContentTypeRefreshedNotification : ContentTypeRefreshNotification<IContentType>
{
    public ContentTypeRefreshedNotification(ContentTypeChange<IContentType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentTypeRefreshedNotification(IEnumerable<ContentTypeChange<IContentType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
