// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before media is saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation.
///     The notification is published by the <see cref="Services.IMediaService"/> before media is persisted.
/// </remarks>
public sealed class MediaSavingNotification : SavingNotification<IMedia>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaSavingNotification"/> class with a single media item.
    /// </summary>
    /// <param name="target">The media item being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaSavingNotification(IMedia target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaSavingNotification"/> class with multiple media items.
    /// </summary>
    /// <param name="target">The collection of media items being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaSavingNotification(IEnumerable<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
