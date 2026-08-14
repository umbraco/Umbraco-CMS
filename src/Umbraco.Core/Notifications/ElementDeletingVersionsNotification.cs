using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IElementService when the DeleteVersion and DeleteVersions methods are called in the API.
/// </summary>
public sealed class ElementDeletingVersionsNotification : DeletingVersionsNotification<IElement>
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="ElementDeletingVersionsNotification"/>.
    /// </summary>
    /// <param name="id">
    /// Gets the ID of the <see cref="IElement"/> object being deleted.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    /// <param name="specificVersion">
    ///  Gets the id of the IElement object version being deleted.
    /// </param>
    /// <param name="deletePriorVersions">
    ///  False by default.
    /// </param>
    /// <param name="dateToRetain">
    /// Gets the latest version date.
    /// </param>
    public ElementDeletingVersionsNotification(int id, EventMessages messages, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default)
        : base(id, messages, specificVersion, deletePriorVersions, dateToRetain)
    {
    }
}
