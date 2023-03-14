using System.ComponentModel;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

[Obsolete("This is only used for the internal cache and will change, use tree change notifications instead")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class MediaTypeRefreshedNotification : ContentTypeRefreshNotification<IMediaType>
{
    public MediaTypeRefreshedNotification(ContentTypeChange<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MediaTypeRefreshedNotification(IEnumerable<ContentTypeChange<IMediaType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
