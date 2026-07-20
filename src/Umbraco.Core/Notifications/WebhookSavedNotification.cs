// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after webhooks have been saved.
/// </summary>
/// <remarks>
///     This notification is published after webhooks have been successfully saved,
///     allowing handlers to react for auditing or cache invalidation purposes.
/// </remarks>
public class WebhookSavedNotification : SavedNotification<IWebhook>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WebhookSavedNotification"/> class
    ///     with a single webhook.
    /// </summary>
    /// <param name="target">The webhook that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public WebhookSavedNotification(IWebhook target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebhookSavedNotification"/> class
    ///     with multiple webhooks.
    /// </summary>
    /// <param name="target">The webhooks that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public WebhookSavedNotification(IEnumerable<IWebhook> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
