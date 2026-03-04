// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before webhooks are saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class WebhookSavingNotification : SavingNotification<IWebhook>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WebhookSavingNotification"/> class
    ///     with a single webhook.
    /// </summary>
    /// <param name="target">The webhook being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public WebhookSavingNotification(IWebhook target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebhookSavingNotification"/> class
    ///     with multiple webhooks.
    /// </summary>
    /// <param name="target">The webhooks being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public WebhookSavingNotification(IEnumerable<IWebhook> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
