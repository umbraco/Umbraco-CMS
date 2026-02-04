// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that is used to trigger the IContentService when the SendToPublication method is called in the API.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the send to publish operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public sealed class ContentSendingToPublishNotification : CancelableObjectNotification<IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSendingToPublishNotification"/> class.
    /// </summary>
    /// <param name="target">The content being sent to publish.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentSendingToPublishNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Getting the IContent object being sent to publish.
    /// </summary>
    public IContent Entity => Target;
}
