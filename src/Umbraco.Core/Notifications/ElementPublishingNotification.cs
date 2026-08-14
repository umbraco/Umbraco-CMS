using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IElementService when the Publishing method is called in the API.
/// Called while publishing an element but before the element has been published. Cancel the operation to prevent the publish.
/// </summary>
public sealed class ElementPublishingNotification : CancelableEnumerableObjectNotification<IElement>
{
    public ElementPublishingNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ElementPublishingNotification(IEnumerable<IElement> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    /// Gets a enumeration of <see cref="IElement"/> which are being published.
    /// </summary>
    public IEnumerable<IElement> PublishedEntities => Target;
}
