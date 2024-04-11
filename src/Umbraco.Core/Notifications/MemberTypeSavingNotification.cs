using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMemberTypeService when the Save method is called in the API.
/// </summary>
public class MemberTypeSavingNotification : SavingNotification<IMemberType>
{
    public MemberTypeSavingNotification(IMemberType target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberTypeSavingNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of <see cref="IMemberType"/> objects being saved.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MemberTypeSavingNotification(IEnumerable<IMemberType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
