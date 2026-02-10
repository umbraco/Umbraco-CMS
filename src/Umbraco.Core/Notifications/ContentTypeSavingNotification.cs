// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before a content type is saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation.
///     The notification is published by the <see cref="Services.IContentTypeService"/> before the content type is persisted.
/// </remarks>
public class ContentTypeSavingNotification : SavingNotification<IContentType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeSavingNotification"/> class with a single content type.
    /// </summary>
    /// <param name="target">The content type being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeSavingNotification(IContentType target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeSavingNotification"/> class with multiple content types.
    /// </summary>
    /// <param name="target">The collection of content types being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeSavingNotification(IEnumerable<IContentType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
