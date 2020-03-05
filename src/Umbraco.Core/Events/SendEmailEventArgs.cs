using System;
using System.Net.Mail;

namespace Umbraco.Core.Events
{
    public class SendEmailEventArgs : EventArgs
    {
        public MailMessage Message { get; private set; }

        public SendEmailEventArgs(MailMessage message)
        {
            Message = message;
        }
    }
}
