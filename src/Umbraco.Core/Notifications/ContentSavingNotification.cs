// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before content is saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation.
///     The notification is published by the <see cref="Services.IContentService"/> before content is persisted.
/// </remarks>
public sealed class ContentSavingNotification : SavingNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSavingNotification"/> class with a single content item.
    /// </summary>
    /// <param name="target">The content item being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentSavingNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSavingNotification"/> class with multiple content items.
    /// </summary>
    /// <param name="target">The collection of content items being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentSavingNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
