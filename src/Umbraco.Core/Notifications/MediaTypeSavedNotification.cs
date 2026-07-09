// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a media type has been saved.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IMediaTypeService"/> after the media type has been persisted.
///     It is not cancelable since the save operation has already completed.
/// </remarks>
public class MediaTypeSavedNotification : SavedNotification<IMediaType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTypeSavedNotification"/> class with a single media type.
    /// </summary>
    /// <param name="target">The media type that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaTypeSavedNotification(IMediaType target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTypeSavedNotification"/> class with multiple media types.
    /// </summary>
    /// <param name="target">The collection of media types that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaTypeSavedNotification(IEnumerable<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
