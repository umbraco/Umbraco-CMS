using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMemberTypeService when the Delete method is called in the API.
/// </summary>
public class MemberTypeDeletingNotification : DeletingNotification<IMemberType>
{
    public MemberTypeDeletingNotification(IMemberType target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberTypeDeletingNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of <see cref="IMemberType"/> objects being deleted.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MemberTypeDeletingNotification(IEnumerable<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
