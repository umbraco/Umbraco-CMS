using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IElementService when the Save method is called in the API and after the data has been persisted.
/// </summary>
public sealed class ElementSavedNotification : SavedNotification<IElement>
{
    /// <summary>
    ///  Initializes a new instance of the  <see cref="ElementSavedNotification"/>
    /// </summary>
    public ElementSavedNotification(IElement target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    /// Gets a enumeration of <see cref="IElement"/>.
    /// </summary>
    public ElementSavedNotification(IEnumerable<IElement> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
