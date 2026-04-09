// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before content is rolled back to a previous version.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the rollback operation.
///     The notification is published by the <see cref="Services.IContentService"/> before the content is reverted.
/// </remarks>
public sealed class ContentRollingBackNotification : RollingBackNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentRollingBackNotification"/> class.
    /// </summary>
    /// <param name="target">The content item being rolled back.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentRollingBackNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }
}
