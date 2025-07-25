using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IElementService when the Delete and EmptyRecycleBin methods are called in the API.
/// </summary>
public sealed class ElementDeletedNotification : DeletedNotification<IElement>
{
    public ElementDeletedNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }
}
