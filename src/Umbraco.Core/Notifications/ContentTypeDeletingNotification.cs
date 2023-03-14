using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class ContentTypeDeletingNotification : DeletingNotification<IContentType>
{
    public ContentTypeDeletingNotification(IContentType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentTypeDeletingNotification(IEnumerable<IContentType> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }
}
