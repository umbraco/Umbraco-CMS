using System;
using System.Net.Mail;

namespace Umbraco.Core.Events
{
    public class SendEmailEventArgs : EventArgs
    {
        public SmtpClient Smtp { get; private set; }
        public MailMessage Message { get; private set; }

        ///// <summary>
        ///// A flag indicating that the mail sending was handled by an event handler
        ///// </summary>
        //public bool SendingHandledExternally { get; set; }

        public SendEmailEventArgs(MailMessage message)
        {
            Message = message;
        }

        public SendEmailEventArgs(SmtpClient smtp, MailMessage message)
        {
            Smtp = smtp;
            Message = message;
        }
    }
}