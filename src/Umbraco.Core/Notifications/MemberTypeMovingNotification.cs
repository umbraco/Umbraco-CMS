using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMemberTypeService when the Move method is called in the API.
/// </summary>
public class MemberTypeMovingNotification : MovingNotification<IMemberType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeMovingNotification"/> class
    ///     with a single member type move operation.
    /// </summary>
    /// <param name="target">The move event information for the member type being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberTypeMovingNotification(MoveEventInfo<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeMovingNotification"/> class
    ///     with multiple member type move operations.
    /// </summary>
    /// <param name="target">The move event information for the member types being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MemberTypeMovingNotification(IEnumerable<MoveEventInfo<IMemberType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
