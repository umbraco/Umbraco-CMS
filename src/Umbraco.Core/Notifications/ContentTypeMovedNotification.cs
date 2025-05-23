using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the ContentTypeService when the Move method is called in the API, after the entities have been moved.
/// </summary>
public class ContentTypeMovedNotification : MovedNotification<IContentType>
{
    public ContentTypeMovedNotification(MoveEventInfo<IContentType> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }

    public ContentTypeMovedNotification(IEnumerable<MoveEventInfo<IContentType>> target, EventMessages messages)
        : base(
        target, messages)
    {
    }
}
