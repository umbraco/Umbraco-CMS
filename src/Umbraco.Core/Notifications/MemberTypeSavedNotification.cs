using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMemberTypeService when the Save method is called in the API, after the entities have been saved.
/// </summary>
public class MemberTypeSavedNotification : SavedNotification<IMemberType>
{
    public MemberTypeSavedNotification(IMemberType target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberTypeSavedNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of saved <see cref="IMemberType"/> objects.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MemberTypeSavedNotification(IEnumerable<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
