// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after the media recycle bin has been emptied.
/// </summary>
/// <remarks>
///     This notification is published after all media items in the recycle bin have been
///     permanently deleted.
/// </remarks>
public sealed class MediaEmptiedRecycleBinNotification : EmptiedRecycleBinNotification<IMedia>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaEmptiedRecycleBinNotification"/> class.
    /// </summary>
    /// <param name="deletedEntities">The media items that were permanently deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaEmptiedRecycleBinNotification(IEnumerable<IMedia> deletedEntities, EventMessages messages)
        : base(deletedEntities, messages)
    {
    }
}
