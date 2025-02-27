using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMediaTypeService when the Delete method is called in the API, after the entities has been deleted.
/// </summary>
public class MediaTypeDeletedNotification : DeletedNotification<IMediaType>
{
    public MediaTypeDeletedNotification(IMediaType target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeDeletedNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of deleted <see cref="IMediaType"/> objects.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MediaTypeDeletedNotification(IEnumerable<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
