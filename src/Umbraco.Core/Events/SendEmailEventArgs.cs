using System;
using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Core.Events
{
    public class SendEmailEventArgs : EventArgs
    {
        public EmailMessage Message { get; }

        public SendEmailEventArgs(EmailMessage message)
        {
            Message = message;
        }
    }
}
