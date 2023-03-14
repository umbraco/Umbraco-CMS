using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class ContentTypeSavingNotification : SavingNotification<IContentType>
{
    public ContentTypeSavingNotification(IContentType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentTypeSavingNotification(IEnumerable<IContentType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
