// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after content has been rolled back to a previous version.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IContentService"/> after the content has been reverted.
///     It is not cancelable since the rollback operation has already completed.
/// </remarks>
public sealed class ContentRolledBackNotification : RolledBackNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentRolledBackNotification"/> class.
    /// </summary>
    /// <param name="target">The content item that was rolled back.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentRolledBackNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }
}
