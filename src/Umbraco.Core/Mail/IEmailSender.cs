using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Core.Mail;

/// <summary>
///     Simple abstraction to send an email message
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends a message asynchronously.
    /// </summary>
    Task SendAsync(EmailMessage message, string emailType, bool enableNotification = false, TimeSpan? expires = null);

    /// <summary>
    /// Verifies if the email sender is configured to send emails.
    /// </summary>
    bool CanSendRequiredEmail();
}
