using System;
using Umbraco.Cms.Core.Models;

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
