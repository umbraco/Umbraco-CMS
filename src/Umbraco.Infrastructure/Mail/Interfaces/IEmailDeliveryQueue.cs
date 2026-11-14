using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Infrastructure.Mail.Interfaces;

/// <summary>
///     Accepts emails for later delivery through the <see cref="IEmailSenderClient"/>, so that the sender does not wait
///     for the delivery to complete.
/// </summary>
/// <remarks>
///     Used when <see cref="Core.Configuration.Models.SmtpSettings.SendInBackground"/> is enabled. An implementation owns
///     how queued emails are stored, which emails it accepts and when they are delivered; the default holds them in memory,
///     within the limits set in <see cref="Core.Configuration.Models.SmtpSettings"/>, and delivers them in the order they
///     were queued. Replace it to, for example, persist queued emails so that they survive a restart.
///     Delivery runs outside the request that queued the email, so the <see cref="IEmailSenderClient"/> cannot rely on the
///     current request or on scoped services from it.
/// </remarks>
public interface IEmailDeliveryQueue
{
    /// <summary>
    ///     Queues an email for delivery, if the queue can accept it.
    /// </summary>
    /// <param name="message">The email to deliver.</param>
    /// <param name="emailType">The type of email, for diagnostics.</param>
    /// <param name="expires">An optional time after which the email expires.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     <c>true</c> if the email was queued; <c>false</c> if the queue cannot accept it, in which case the sender sends it
    ///     immediately instead.
    /// </returns>
    /// <remarks>
    ///     The sender may dispose the message's attachment streams as soon as this completes, so an implementation that
    ///     accepts the email must first take its own copy of anything it needs from them.
    /// </remarks>
    Task<bool> TryEnqueueAsync(EmailMessage message, string emailType, TimeSpan? expires, CancellationToken cancellationToken = default);
}
