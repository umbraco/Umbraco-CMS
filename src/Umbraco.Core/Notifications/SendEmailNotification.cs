using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Core.Notifications;

public class SendEmailNotification : INotification
{
    public SendEmailNotification(NotificationEmailModel message, string emailType)
    {
        Message = message;
        EmailType = emailType;
    }

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
