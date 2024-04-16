// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMediaService when the DeleteVersion and DeleteVersions methods are called in the API.
/// </summary>
public sealed class MediaDeletingVersionsNotification : DeletingVersionsNotification<IMedia>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaDeletingVersionsNotification"/>.
    /// </summary>
    /// <param name="id">
    /// Gets the id of the <see cref="IMedia"/> object being deleted.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    /// <param name="specificVersion">
    /// Gets the id of the <see cref="IMedia"/> object version being deleted.
    /// </param>
    /// <param name="deletePriorVersions">
    /// False by default.
    /// </param>
    /// <param name="dateToRetain">
    /// Gets the latest version date.
    /// </param>
    public MediaDeletingVersionsNotification(int id, EventMessages messages, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default)
        : base(id, messages, specificVersion, deletePriorVersions, dateToRetain)
    {
    }
}
