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
    [Obsolete("Please use IsEmailConfigured to check configuration only, or IsEmailAvailableAsync to also check that the transport can currently be reached. Scheduled for removal in Umbraco 19.")]
    bool CanSendRequiredEmail();

    /// <summary>
    /// Determines whether the email sender is configured to send emails.
    /// </summary>
    /// <returns><c>true</c> if the email sender is configured; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This checks configuration only and performs no I/O, so it is suitable for deciding which features to offer.
    /// Use <see cref="IsEmailAvailableAsync"/> to check that the transport can be reached.
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
    /// <see cref="IsEmailConfigured"/> unless an email is about to be sent.
    /// </remarks>
    // TODO (V19): Remove the default implementation.
    Task<bool> IsEmailAvailableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(IsEmailConfigured());
}
