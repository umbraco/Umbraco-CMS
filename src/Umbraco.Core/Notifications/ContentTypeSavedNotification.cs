using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the ContentTypeService when the Save method is called in the API, after the entities have been saved.
/// </summary>
public class ContentTypeSavedNotification : SavedNotification<IContentType>
{
    public ContentTypeSavedNotification(IContentType target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentTypeSavedNotification(IEnumerable<IContentType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
