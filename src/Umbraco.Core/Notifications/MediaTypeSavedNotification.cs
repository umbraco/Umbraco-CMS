using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMediaTypeService when the Save method is called in the API, after the entities has been saved.
/// </summary>
public class MediaTypeSavedNotification : SavedNotification<IMediaType>
{
    public MediaTypeSavedNotification(IMediaType target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeSavedNotification"/>.
    /// </summary>
    /// <param name="target">
    /// Gets the collection of saved <see cref="IMediaType"/> objects.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MediaTypeSavedNotification(IEnumerable<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
