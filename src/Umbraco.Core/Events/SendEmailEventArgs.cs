using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Events
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
