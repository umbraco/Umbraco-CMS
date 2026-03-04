// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Base class for notifications that can be canceled by a notification handler.
/// </summary>
/// <remarks>
///     Notification handlers can set <see cref="Cancel"/> to <c>true</c> or call <see cref="CancelOperation"/>
///     to abort the operation that triggered the notification.
/// </remarks>
public class CancelableNotification : StatefulNotification, ICancelableNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CancelableNotification"/> class.
    /// </summary>
    /// <param name="messages">The event messages collection for adding cancellation messages.</param>
    public CancelableNotification(EventMessages messages) => Messages = messages;

    /// <summary>
    ///     Gets the event messages collection associated with this notification.
    /// </summary>
    public EventMessages Messages { get; }

    /// <inheritdoc />
    public bool Cancel { get; set; }

    /// <summary>
    ///     Cancels the operation and adds a cancellation message to the event messages collection.
    /// </summary>
    /// <param name="cancellationMessage">The message explaining why the operation was canceled.</param>
    public void CancelOperation(EventMessage cancellationMessage)
    {
        Cancel = true;
        cancellationMessage.IsDefaultEventMessage = true;
        Messages.Add(cancellationMessage);
    }
}
