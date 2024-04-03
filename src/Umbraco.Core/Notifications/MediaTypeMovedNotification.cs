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
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeMovedNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the <see cref="IMediaType"/> object being moved.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MediaTypeMovedNotification(IEnumerable<MoveEventInfo<IMediaType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
