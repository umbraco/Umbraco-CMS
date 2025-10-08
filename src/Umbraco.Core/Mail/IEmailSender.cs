using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Core.Mail;

/// <summary>
///     Simple abstraction to send an email message
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, string emailType);

    Task SendAsync(EmailMessage message, string emailType, TimeSpan? expires);

    Task SendAsync(EmailMessage message, string emailType, bool enableNotification);

    Task SendAsync(EmailMessage message, string emailType, bool enableNotification, TimeSpan? expires);

    bool CanSendRequiredEmail();
}
