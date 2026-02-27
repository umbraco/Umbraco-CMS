// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before the media recycle bin is emptied.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the recycle bin
///     from being emptied by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public sealed class MediaEmptyingRecycleBinNotification : EmptyingRecycleBinNotification<IMedia>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaEmptyingRecycleBinNotification"/> class.
    /// </summary>
    /// <param name="deletedEntities">The media items that will be permanently deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaEmptyingRecycleBinNotification(IEnumerable<IMedia> deletedEntities, EventMessages messages)
        : base(deletedEntities, messages)
    {
    }
}
