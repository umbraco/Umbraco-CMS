using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the ContentTypeService when the Save method is called in the API.
/// </summary>
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
