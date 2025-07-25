using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IElementService when the DeleteElementOfType, Delete and EmptyRecycleBin methods are called in the API.
/// </summary>
public sealed class ElementDeletingNotification : DeletingNotification<IElement>
{
    public ElementDeletingNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ElementDeletingNotification(IEnumerable<IElement> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
