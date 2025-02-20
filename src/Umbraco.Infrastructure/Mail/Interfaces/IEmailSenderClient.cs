using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Infrastructure.Mail.Interfaces
{
    /// <summary>
    /// Client for sending an email from a MimeMessage
    /// </summary>
    public interface IEmailSenderClient
    {
        /// <summary>
        /// Sends the email message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task SendAsync(EmailMessage message);
    }
}
