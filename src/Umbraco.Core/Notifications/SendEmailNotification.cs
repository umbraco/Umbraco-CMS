// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when an email needs to be sent.
/// </summary>
/// <remarks>
///     Handlers can intercept this notification to provide custom email sending logic
///     or to modify the email before it is sent. Call <see cref="HandleEmail"/> to indicate
///     that the email has been handled and should not be processed by the default handler.
/// </remarks>
public class SendEmailNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SendEmailNotification"/> class.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <param name="emailType">Metadata about the type of email being sent.</param>
    public SendEmailNotification(NotificationEmailModel message, string emailType)
    {
        Message = message;
        EmailType = emailType;
    }

    /// <summary>
    ///     Gets the email message to send.
    /// </summary>
    public NotificationEmailModel Message { get; }

    /// <summary>
    ///     Some metadata about the email which can be used by handlers to determine if they should handle the email or not
    /// </summary>
    public string EmailType { get; }

    /// <summary>
    ///     Returns true if the email sending is handled.
    /// </summary>
    public bool IsHandled { get; private set; }

    /// <summary>
    ///     Call to tell Umbraco that the email sending is handled.
    /// </summary>
    public void HandleEmail() => IsHandled = true;
}
