using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IElementService when the UnPublishing method is called in the API.
/// </summary>
public sealed class ElementUnpublishingNotification : CancelableEnumerableObjectNotification<IElement>
{
    public ElementUnpublishingNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ElementUnpublishingNotification(IEnumerable<IElement> target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Gets a enumeration of <see cref="IElement"/> which are being unpublished.
    /// </summary>
    public IEnumerable<IElement> UnpublishedEntities => Target;
}
