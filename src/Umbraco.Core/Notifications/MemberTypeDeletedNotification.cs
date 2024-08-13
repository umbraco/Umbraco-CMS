using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMemberTypeService when the Delete method is called in the API, after the entities have been deleted.
/// </summary>
public class MemberTypeDeletedNotification : DeletedNotification<IMemberType>
{
    public MemberTypeDeletedNotification(IMemberType target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberTypeDeletedNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of deleted <see cref="IMemberType"/> objects.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MemberTypeDeletedNotification(IEnumerable<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
