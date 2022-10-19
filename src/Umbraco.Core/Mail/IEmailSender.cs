using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Core.Mail;

/// <summary>
///     Simple abstraction to send an email message
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, string emailType);

    Task SendAsync(EmailMessage message, string emailType, bool enableNotification);

    bool CanSendRequiredEmail();
}
