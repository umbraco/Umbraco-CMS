using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMediaTypeService when the Move method is called in the API.
/// </summary>
public class MediaTypeMovingNotification : MovingNotification<IMediaType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTypeMovingNotification"/> class
    ///     with a single media type move operation.
    /// </summary>
    /// <param name="target">The move event information for the media type being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaTypeMovingNotification(MoveEventInfo<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeMovingNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the <see cref="IMediaType"/> object being moved.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MediaTypeMovingNotification(IEnumerable<MoveEventInfo<IMediaType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
