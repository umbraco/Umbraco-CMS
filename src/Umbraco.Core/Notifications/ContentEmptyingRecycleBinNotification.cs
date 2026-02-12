// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before the content recycle bin is emptied.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the empty recycle bin operation.
///     The notification is published by the <see cref="Services.IContentService"/> when EmptyRecycleBin is called.
/// </remarks>
public sealed class ContentEmptyingRecycleBinNotification : EmptyingRecycleBinNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentEmptyingRecycleBinNotification"/> class.
    /// </summary>
    /// <param name="deletedEntities">The collection of content items being permanently deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentEmptyingRecycleBinNotification(IEnumerable<IContent>? deletedEntities, EventMessages messages)
        : base(
        deletedEntities, messages)
    {
    }
}
