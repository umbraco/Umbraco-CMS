using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMediaTypeService when the Move method is called in the API, after the entities has been moved.
/// </summary>
public class MediaTypeMovedNotification : MovedNotification<IMediaType>
{
    public MediaTypeMovedNotification(MoveEventInfo<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MediaTypeMovedNotification(IEnumerable<MoveEventInfo<IMediaType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
