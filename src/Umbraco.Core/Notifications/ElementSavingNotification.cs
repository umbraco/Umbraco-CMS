using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IElementService when the Save method is called in the API.
/// </summary>
public sealed class ElementSavingNotification : SavingNotification<IElement>
{
    /// <summary>
    ///  Initializes a new instance of the  <see cref="Umbraco.Cms.Core.Notifications.ElementSavingNotification"/>
    /// </summary>
    public ElementSavingNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    /// Gets a enumeration of <see cref="IElement"/>.
    /// </summary>
    public ElementSavingNotification(IEnumerable<IElement> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
