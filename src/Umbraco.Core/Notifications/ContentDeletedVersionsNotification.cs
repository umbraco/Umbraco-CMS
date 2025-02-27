// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the DeleteVersion and DeleteVersions methods are called in the API, and the version has been deleted.
/// </summary>
public sealed class ContentDeletedVersionsNotification : DeletedVersionsNotification<IContent>
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="ContentDeletedVersionsNotification"/>.
    /// </summary>
    /// <param name="id">
    /// Gets the ID of the <see cref="IContent"/> object being deleted.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    /// <param name="specificVersion">
    ///  Gets the id of the IContent object version being deleted.
    /// </param>
    /// <param name="deletePriorVersions">
    ///  False by default.
    /// </param>
    /// <param name="dateToRetain">
    /// Gets the latest version date.
    /// </param>
    public ContentDeletedVersionsNotification(
        int id,
        EventMessages messages,
        int specificVersion = default,
        bool deletePriorVersions = false,
        DateTime dateToRetain = default)
        : base(id, messages, specificVersion, deletePriorVersions, dateToRetain)
    {
    }
}
