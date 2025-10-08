using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Core.Mail;

/// <summary>
///     Simple abstraction to send an email message
/// </summary>
public interface IEmailSender
{
    [Obsolete("Use the overload with expires parameter.")]
    Task SendAsync(EmailMessage message, string emailType);

    Task SendAsync(EmailMessage message, string emailType, TimeSpan? expires)
#pragma warning disable CS0618 // Type or member is obsolete
        => SendAsync(message, emailType);
#pragma warning restore CS0618 // Type or member is obsolete

    [Obsolete("Use the overload with expires parameter.")]
    Task SendAsync(EmailMessage message, string emailType, bool enableNotification);

    Task SendAsync(EmailMessage message, string emailType, bool enableNotification, TimeSpan? expires)
#pragma warning disable CS0618 // Type or member is obsolete
        => SendAsync(message, emailType, enableNotification);
#pragma warning restore CS0618 // Type or member is obsolete

    bool CanSendRequiredEmail();
}
