using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

public class ContentTypeChangedNotification : ContentTypeChangeNotification<IContentType>
{
    public ContentTypeChangedNotification(ContentTypeChange<IContentType> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }

    public ContentTypeChangedNotification(IEnumerable<ContentTypeChange<IContentType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
