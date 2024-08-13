using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the ContentTypeService when the Delete method is called in the API, after the entities have been deleted.
/// </summary>
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
