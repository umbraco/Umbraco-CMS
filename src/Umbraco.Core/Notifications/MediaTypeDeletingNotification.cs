using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMediaTypeService when the Delete method is called in the API.
/// </summary>
public class MediaTypeDeletingNotification : DeletingNotification<IMediaType>
{
    public MediaTypeDeletingNotification(IMediaType target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeDeletingNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of <see cref="IMediaType"/> objects being deleted.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MediaTypeDeletingNotification(IEnumerable<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
