using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the IElementService when the DeleteVersion and DeleteVersions methods are called in the API, and the version has been deleted.
/// </summary>
public sealed class ElementDeletedVersionsNotification : DeletedVersionsNotification<IElement>
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="ElementDeletedVersionsNotification"/>.
    /// </summary>
    /// <param name="key">
    /// Gets the Guid key of the <see cref="IElement"/> object being deleted.
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
    public ElementDeletedVersionsNotification(
        Guid key,
        EventMessages messages,
        int specificVersion = default,
        bool deletePriorVersions = false,
        DateTime dateToRetain = default)
        : base(key, messages, specificVersion, deletePriorVersions, dateToRetain)
    {
    }
}
