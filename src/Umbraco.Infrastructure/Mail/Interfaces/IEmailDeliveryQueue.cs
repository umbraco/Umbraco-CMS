using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Infrastructure.Mail.Interfaces;

/// <summary>
///     Accepts emails for later delivery through the <see cref="IEmailSenderClient"/>, so that the sender does not wait
///     for the delivery to complete.
/// </summary>
/// <remarks>
///     Used when <see cref="Core.Configuration.Models.SmtpSettings.SendInBackground"/> is enabled. An implementation owns
///     how queued emails are stored and when they are delivered; the default holds them in memory and delivers them in
///     the order they were queued. Replace it to, for example, persist queued emails so that they survive a restart.
/// </remarks>
public interface IEmailDeliveryQueue
{
    /// <summary>
    ///     Queues an email for delivery.
    /// </summary>
    /// <param name="message">The email to deliver.</param>
    /// <param name="emailType">The type of email, for diagnostics.</param>
    /// <param name="expires">An optional time after which the email expires.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the email has been queued.</returns>
    ValueTask EnqueueAsync(EmailMessage message, string emailType, TimeSpan? expires, CancellationToken cancellationToken = default);
}
