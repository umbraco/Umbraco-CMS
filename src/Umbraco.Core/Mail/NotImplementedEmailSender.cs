using System;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Mail
{
    internal class NotImplementedEmailSender : IEmailSender
    {
        public Task SendAsync(EmailMessage message)
            => throw new NotImplementedException("To send an Email ensure IEmailSender is implemented with a custom implementation");
    }
}
