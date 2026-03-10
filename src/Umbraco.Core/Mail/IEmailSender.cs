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
    [Obsolete("Please use the overload with expires parameter. Scheduled for removal in Umbraco 18.")]
    Task SendAsync(EmailMessage message, string emailType);

    /// <summary>
    /// Sends a message asynchronously.
    /// </summary>
    [Obsolete("Please use the overload with expires parameter. Scheduled for removal in Umbraco 18.")]
    Task SendAsync(EmailMessage message, string emailType, bool enableNotification);

    /// <summary>
    /// Sends a message asynchronously.
    /// </summary>
    Task SendAsync(EmailMessage message, string emailType, bool enableNotification = false, TimeSpan? expires = null)
#pragma warning disable CS0618 // Type or member is obsolete
        => SendAsync(message, emailType, enableNotification);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Verifies if the email sender is configured to send emails.
    /// </summary>
    bool CanSendRequiredEmail();
}
