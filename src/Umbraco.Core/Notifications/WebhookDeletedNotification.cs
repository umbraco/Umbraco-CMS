// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after webhooks have been deleted.
/// </summary>
/// <remarks>
///     This notification is published after webhooks have been successfully deleted,
///     allowing handlers to react for auditing or cache invalidation purposes.
/// </remarks>
public class WebhookDeletedNotification : DeletedNotification<IWebhook>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WebhookDeletedNotification"/> class.
    /// </summary>
    /// <param name="target">The webhook that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public WebhookDeletedNotification(IWebhook target, EventMessages messages)
        : base(target, messages)
    {
    }
}
