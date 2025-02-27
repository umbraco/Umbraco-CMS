using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMediaTypeService when the Save method is called in the API.
/// </summary>
public class MediaTypeSavingNotification : SavingNotification<IMediaType>
{
    public MediaTypeSavingNotification(IMediaType target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeSavingNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of <see cref="IMediaType"/> objects being saved.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MediaTypeSavingNotification(IEnumerable<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
