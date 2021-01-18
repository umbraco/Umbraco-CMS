using System;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Umbraco.Core.Mail
{
    internal class NotImplementedEmailSender : IEmailSender
    {
        public Task SendAsync(EmailMessage message)
            => throw new NotImplementedException("To send an Email ensure IEmailSender is implemented with a custom implementation");
    }
}
