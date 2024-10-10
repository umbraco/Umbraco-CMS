// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMediaService when the DeleteMediaOfType, Delete and EmptyRecycleBin methods are called in the API.
/// </summary>
public sealed class MediaDeletingNotification : DeletingNotification<IMedia>
{
    public MediaDeletingNotification(IMedia target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaDeletingNotification"/>.
    /// </summary>
    /// <param name="target">
    ///  Gets the collection of <see cref="IMedia"/> objects being deleted.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MediaDeletingNotification(IEnumerable<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
