using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class ContentTypeDeletedNotification : DeletedNotification<IContentType>
{
    public ContentTypeDeletedNotification(IContentType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentTypeDeletedNotification(IEnumerable<IContentType> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }
}
