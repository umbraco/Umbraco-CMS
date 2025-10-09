using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Infrastructure.Mail.Interfaces
{
    /// <summary>
    /// Client for sending an email from a MimeMessage.
    /// </summary>
    public interface IEmailSenderClient
    {
        /// <summary>
        /// Sends the email message.
        /// </summary>
        /// <param name="message">The <see cref="EmailMessage"/> to send.</param>
        [Obsolete("Please use the overload taking all parameters. Scheduled for removal in Umbraco 18.")]
        public Task SendAsync(EmailMessage message);

        /// <summary>
        /// Sends the email message with an expiration date.
        /// </summary>
        /// <param name="message">The <see cref="EmailMessage"/> to send.</param>
        /// <param name="expires">An optional time for expiry.</param>
        public Task SendAsync(EmailMessage message, TimeSpan? expires)
#pragma warning disable CS0618 // Type or member is obsolete
            => SendAsync(message);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
