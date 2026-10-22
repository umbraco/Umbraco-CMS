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
    /// <param name="message">The message to send.</param>
    /// <param name="emailType">The type of email, which notification handlers can use to decide whether to handle it.</param>
    /// <param name="enableNotification">
    /// Whether to publish the <see cref="Notifications.SendEmailNotification"/> so that a handler can send the email instead.
    /// The notification is published regardless when <see cref="Configuration.Models.SmtpSettings.PublishNotificationForAllEmails"/>
    /// is enabled.
    /// </param>
    /// <param name="expires">An optional time after which the email expires.</param>
    // TODO (V20): The notification is always published, so add overloads without enableNotification (avoiding optional
    // parameters, so calls don't bind ambiguously), obsolete this one and move internal callers to them.
    Task SendAsync(EmailMessage message, string emailType, bool enableNotification = false, TimeSpan? expires = null)
#pragma warning disable CS0618 // Type or member is obsolete
        => SendAsync(message, emailType, enableNotification);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Verifies if the email sender is configured to send emails.
    /// </summary>
    [Obsolete("Please use IsEmailConfigured to check configuration only, or IsEmailAvailableAsync to also check that the transport can currently be reached. Scheduled for removal in Umbraco 19.")]
    bool CanSendRequiredEmail();

    /// <summary>
    /// Determines whether the email sender is configured to send emails.
    /// </summary>
    /// <returns><c>true</c> if the email sender is configured; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This checks configuration only and performs no I/O, so it is suitable for deciding which features to offer.
    /// Use <see cref="IsEmailAvailableAsync"/> to check that the transport can be reached. A registered
    /// <see cref="Notifications.SendEmailNotification"/> handler can only send the emails the notification is published for.
    /// </remarks>
    // TODO (V19): Remove the default implementation when CanSendRequiredEmail is removed.
    bool IsEmailConfigured()
#pragma warning disable CS0618 // Type or member is obsolete
        => CanSendRequiredEmail();
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Determines whether the email sender is configured to send emails and its transport can currently be reached.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if emails can currently be sent; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Implementations may perform network I/O, such as connecting to a mail server, so prefer
    /// <see cref="IsEmailConfigured"/> unless an email is about to be sent. Implementations may cache the result briefly.
    /// </remarks>
    // TODO (V19): Remove the default implementation.
    Task<bool> IsEmailAvailableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(IsEmailConfigured());
}
