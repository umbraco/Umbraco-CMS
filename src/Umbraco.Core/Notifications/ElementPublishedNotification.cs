using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IElementService when the Publish method is called in the API and after data has been published.
/// Called after an element has been published.
/// </summary>
public sealed class ElementPublishedNotification : EnumerableObjectNotification<IElement>
{
    public ElementPublishedNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ElementPublishedNotification(IEnumerable<IElement> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    /// Gets a enumeration of <see cref="IElement"/> which are being published.
    /// </summary>
    public IEnumerable<IElement> PublishedEntities => Target;
}
