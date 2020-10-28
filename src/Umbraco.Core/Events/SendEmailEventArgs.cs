using System;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Models;

namespace Umbraco.Core.Events
{
    [UmbracoVolatile]
    public class SendEmailEventArgs : EventArgs
    {
        public EmailMessage Message { get; }

        public SendEmailEventArgs(EmailMessage message)
        {
            Message = message;
        }
    }
}
