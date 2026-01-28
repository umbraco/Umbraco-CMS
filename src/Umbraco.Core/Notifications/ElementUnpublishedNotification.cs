using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IElementService when the UnPublish method is called in the API and after data has been unpublished.
/// </summary>
public sealed class ElementUnpublishedNotification : EnumerableObjectNotification<IElement>
{
    public ElementUnpublishedNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ElementUnpublishedNotification(IEnumerable<IElement> target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Gets a enumeration of <see cref="IElement"/> which are being unpublished.
    /// </summary>
    public IEnumerable<IElement> UnpublishedEntities => Target;
}
