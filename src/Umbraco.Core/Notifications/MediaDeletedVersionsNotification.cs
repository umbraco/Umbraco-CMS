// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMediaService when the DeleteVersion and DeleteVersions methods are called in the API, after the media version has been deleted.
/// </summary>
public sealed class MediaDeletedVersionsNotification : DeletedVersionsNotification<IMedia>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaDeletingVersionsNotification"/>.
    /// </summary>
    /// <param name="id">
    /// Gets the id of the deleted <see cref="IMedia"/> object.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    /// <param name="specificVersion">
    /// Gets the id of the deleted <see cref="IMedia"/> object version.
    /// </param>
    /// <param name="deletePriorVersions">
    /// False by default.
    /// </param>
    /// <param name="dateToRetain">
    /// Gets the latest version date.
    /// </param>
    public MediaDeletedVersionsNotification(int id, EventMessages messages, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default)
        : base(id, messages, specificVersion, deletePriorVersions, dateToRetain)
    {
    }
}
