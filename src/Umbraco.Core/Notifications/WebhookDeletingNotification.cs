// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before webhooks are deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class WebhookDeletingNotification : DeletingNotification<IWebhook>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WebhookDeletingNotification"/> class
    ///     with a single webhook.
    /// </summary>
    /// <param name="target">The webhook being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public WebhookDeletingNotification(IWebhook target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebhookDeletingNotification"/> class
    ///     with multiple webhooks.
    /// </summary>
    /// <param name="target">The webhooks being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public WebhookDeletingNotification(IEnumerable<IWebhook> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
