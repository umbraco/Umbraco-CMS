using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the ContentTypeService when the Move method is called in the API.
/// </summary>
public class ContentTypeMovingNotification : MovingNotification<IContentType>
{
    public ContentTypeMovingNotification(MoveEventInfo<IContentType> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }

    public ContentTypeMovingNotification(IEnumerable<MoveEventInfo<IContentType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
