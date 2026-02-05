// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after media has been saved.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IMediaService"/> after media has been persisted.
///     It is not cancelable since the save operation has already completed.
/// </remarks>
public sealed class MediaSavedNotification : SavedNotification<IMedia>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaSavedNotification"/> class with a single media item.
    /// </summary>
    /// <param name="target">The media item that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaSavedNotification(IMedia target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaSavedNotification"/> class with multiple media items.
    /// </summary>
    /// <param name="target">The collection of media items that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaSavedNotification(IEnumerable<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
