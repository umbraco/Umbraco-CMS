using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MimeKit;

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
        public Task SendAsync(MimeMessage? message);
    }
}
