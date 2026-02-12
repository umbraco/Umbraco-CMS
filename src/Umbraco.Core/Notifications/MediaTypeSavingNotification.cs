// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before a media type is saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation.
///     The notification is published by the <see cref="Services.IMediaTypeService"/> before the media type is persisted.
/// </remarks>
public class MediaTypeSavingNotification : SavingNotification<IMediaType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTypeSavingNotification"/> class with a single media type.
    /// </summary>
    /// <param name="target">The media type being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaTypeSavingNotification(IMediaType target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTypeSavingNotification"/> class with multiple media types.
    /// </summary>
    /// <param name="target">The collection of media types being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public MediaTypeSavingNotification(IEnumerable<IMediaType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
